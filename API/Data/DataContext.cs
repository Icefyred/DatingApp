using System;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace API.Data{
    //we inherit from IdentityDbContext and all its types
    public class DataContext : IdentityDbContext<AppUser, AppRole, int, IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions options) : base(options){
        }

        //public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes {get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Group> Groups {get; set;}
        public DbSet<Connection> Connections { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder){
            //without this it may returns errors when trying to add a migration 
            base.OnModelCreating(builder);

            //configure the relationship between AppUser and AppRole
            builder.Entity<AppUser>().HasMany(userRole => userRole.UserRoles).WithOne(user => user.User)
                .HasForeignKey(userRole => userRole.UserId).IsRequired();

            builder.Entity<AppRole>().HasMany(userRole => userRole.UserRoles).WithOne(user => user.Role)
                .HasForeignKey(userRole => userRole.RoleId).IsRequired();

            builder.Entity<UserLike>().HasKey(k => new {k.SourceUserId, k.LikedUserId});

            //The delete behaviour means that when the linked user gets deleted so does the liked users that were linked to the user
            //IMPORTANT: If using SQL Server, then set the DeleteBehavior to DeleteBehavior.NoAction, otherwise an error will occur during migration
            builder.Entity<UserLike>().HasOne(s => s.SourceUser).WithMany(l => l.LikedUsers).HasForeignKey(s => s.SourceUserId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>().HasOne(s => s.LikedUser).WithMany(l => l.LikedByUsers).HasForeignKey(s => s.LikedUserId).OnDelete(DeleteBehavior.Cascade);

            //since we don't wandt to delete the messages without the other party have deleted themselves(DeleteBehavior.Restrict).
            builder.Entity<Message>().HasOne(u => u.Recipient).WithMany(m => m.MessagesReceived).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Message>().HasOne(u => u.Sender).WithMany(m => m.MessagesSent).OnDelete(DeleteBehavior.Restrict);

            builder.ApplyUtcDateTimeConverter();
        }
    }

    public static class UtcDateAnnotation{
        private const String IsUtcAnnotation = "IsUtc";
        private static readonly ValueConverter<DateTime, DateTime> UtcConverter =
            new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        private static readonly ValueConverter<DateTime?, DateTime?> UtcNullableConverter =
            new ValueConverter<DateTime?, DateTime?>(v => v, v => v == null ? v : DateTime.SpecifyKind(v.Value, DateTimeKind.Utc));

        public static PropertyBuilder<TProperty> IsUtc<TProperty>(this PropertyBuilder<TProperty> builder, Boolean isUtc = true) =>
            builder.HasAnnotation(IsUtcAnnotation, isUtc);

        public static Boolean IsUtc(this IMutableProperty property) =>
            ((Boolean?)property.FindAnnotation(IsUtcAnnotation)?.Value) ?? true;

        /// <summary>
        /// Make sure this is called after configuring all your entities.
        /// </summary>
        public static void ApplyUtcDateTimeConverter(this ModelBuilder builder)
        {
            foreach (var entityType in builder.Model.GetEntityTypes()){
                foreach (var property in entityType.GetProperties()){
                    if (!property.IsUtc()){
                        continue;
                    }

                    if (property.ClrType == typeof(DateTime)){
                        property.SetValueConverter(UtcConverter);
                    }

                    if (property.ClrType == typeof(DateTime?)){
                    property.SetValueConverter(UtcNullableConverter);
                    }
                }
            }
        }
    }
}