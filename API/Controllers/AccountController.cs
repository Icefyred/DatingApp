using API.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using System.Security.Cryptography;
using System.Text;
using API.DTOs;
using Microsoft.EntityFrameworkCore;
using API.Interfaces;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Username))
            {
                return BadRequest("Username is taken!");
            }
            var user = _mapper.Map<AppUser>(registerDto);

            //using var hmac = new HMACSHA512();

            /*Starter way to create a new user
            var user = new AppUser
            {
                Username = registerDto.Username.ToLower(),
                //since the password is a string, and ComputeHash takes a byte[],
                //it requires the bottom process
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };*/
            user.UserName = registerDto.Username.ToLower();
            //user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
            //user.PasswordSalt = hmac.Key;

           var result = await _userManager.CreateAsync(user, registerDto.Password);

           if(!result.Succeeded) return BadRequest(result.Errors);

           var roleResult = await _userManager.AddToRoleAsync(user, "Member");

           if(!roleResult.Succeeded) return BadRequest(result.Errors);

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateTroken(user),
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(x => x.UserName == loginDto.Username.ToLower());

            if (user == null)
            {
                return Unauthorized("Invalid username");
            }
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if(!result.Succeeded) return Unauthorized();
            /*
            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int idx = 0; idx < computedHash.Length; ++idx){
                if (computedHash[idx] != user.PasswordHash[idx])
                {
                    return Unauthorized("Invalid password");
                }
            }
            */
            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateTroken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }

        private async Task<bool> UserExists(string username)
        {
            //check if there's already an entry
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}