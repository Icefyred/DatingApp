using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            /* we could assemble the GetMemberAsync and select all the properties
            return await _context.Users
                .Where(x => x.Username == username)
                .Select(user => new MemberDto{
                    Id = user.Id,
                    Username = user.Username,
                    ...
                })*/
            return await _context.Users
                .Where(x => x.UserName == username)
                //select what we want from the database mapper
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = _context.Users.AsQueryable();
                //.ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                //this turns off tracking in the entity framework, since we only need to read this
                //.AsNoTracking()
                //.AsQueryable();
            //filtering before sending the query
            query = query.Where(u => u.UserName != userParams.CurrentUserName);
            query = query.Where(u => u.Gender == userParams.Gender);

            var minDateOfBirth = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDateOfBirth = DateTime.Today.AddYears(-userParams.MinAge);

            query = query.Where(u => u.DateOfBirth >= minDateOfBirth && u.DateOfBirth <= maxDateOfBirth);

            query = userParams.OrderBy switch {
                "created" => query.OrderByDescending(u => u.Created),
                //_ means default in this default
                _ => query.OrderByDescending(u => u.LastActive) 
            };

            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(_mapper.
                ConfigurationProvider).AsNoTracking(), 
                userParams.PageNumber, userParams.PageSize);
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            //if there were changes it will return a value greater than 0, which will be true,
            //otherwise the value will be 0 and the statment will return false
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            //This let's entity framework update and change the flag to modified
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}