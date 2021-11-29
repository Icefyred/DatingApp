using Microsoft.AspNetCore.Mvc;
using API.Entities;
using System.Linq;
using API.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using API.Interfaces;
using API.DTOs;
using AutoMapper;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using API.Extensions;
using API.Helpers;

namespace API.Controllers
{

    public class UsersController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IUnitOfWork _unitOfWork;
        public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
        {
            _unitOfWork = unitOfWork;
            _photoService = photoService;
            _mapper = mapper;
        }

        //[Authorize(Roles = "Admin")]
        [HttpGet]
        //Since the API is not able to understand that the UserParams is for a query it requires the annotation [FromQuery]
        //Lesson 158: We'll need to populate currentUsername property into the userParams
        //and set default property to the opposite gender if not specified
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var gender = await _unitOfWork.UserRepository.GetUserGender(User.GetUsername());
            userParams.CurrentUserName = User.GetUsername();
            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = gender == "male" ? "female" : "male";
            }
            var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        //[Authorize(Roles = "Member")]
        //api/users/3
        [Authorize]
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username){
            var currentUsername = User.GetUsername();
            return await _unitOfWork.UserRepository
                .GetMemberAsync(username, isCurrentUser: currentUsername == username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());
            //without this mapper the below commented code would be needed
            _mapper.Map(memberUpdateDto, user);
            /*
            user.City = memberUpdateDto.City;
            user.Country = memberUpdateDto.Country;
            ...
            */
            _unitOfWork.UserRepository.Update(user);
            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            //gets the user
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            //get result back from the photo service
            var result = await _photoService.AddPhotoAsync(file);
            //checks if there's an error with it
            if (result.Error != null) return BadRequest(result.Error.Message);
            //create a new photo
            var photo = new Photo{
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            //checks to see if the user has any photos in their gallery
            /**** Photo Management Challenge 13. ****/
            /*
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }*/

            //adds the photo
            user.Photos.Add(photo);
            //and will add the photo whenever the thread is safe to go
            if (await _unitOfWork.Complete())
            {
                //return _mapper.Map<PhotoDto>(photo); this returns a 200 OK  where it should be a 201 Created
                return CreatedAtRoute("GetUser", new { Username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem adding photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            //gets hold of the photo of the specific id
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await _unitOfWork.Complete()) return NoContent();
            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You cannot delete your main photo");

            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }
            //removes from the database in the API
            user.Photos.Remove(photo);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete the photo");
        }
    }

}