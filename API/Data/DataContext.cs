using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options){
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes {get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder){
            //without this it may returns errors when trying to add a migration 
            base.OnModelCreating(builder);

            builder.Entity<UserLike>().HasKey(k => new {k.SourceUserId, k.LikedUserId});

            //THe delete behaviour means that when the linked user gets deleted so does the liked users that were linked to the user
            //IMPORTANT: If using SQL Server, then set the DeleteBehavior to DeleteBehavior.NoAction, otherwise an error will occur during migration
            builder.Entity<UserLike>().HasOne(s => s.SourceUser).WithMany(l => l.LikedUsers).HasForeignKey(s => s.SourceUserId).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>().HasOne(s => s.LikedUser).WithMany(l => l.LikedByUsers).HasForeignKey(s => s.LikedUserId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}