using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Data;
using DTOs;
using Entities;
using Extensions;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Controllers
{
    [Authorize] // All data in this class is protected by Authorize
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            this.photoService = photoService;
            this.mapper = mapper;
            this.userRepository = userRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            var users = await this.userRepository.GetMembersAsync();

            // map users to memberDto
            // application will take care of mapping memberDto to AppUser
            //var userToReturn = this.mapper.Map<IEnumerable<MemberDto>>(users);
            return Ok(users);
        }

        [HttpGet("{username}", Name = "GetUser")] // the Name parameter is called route name
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await this.userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            // Get username from token
            //var username = User.GetUsername();

            var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

            this.mapper.Map(memberUpdateDto, user);

            this.userRepository.Update(user);

            if (await this.userRepository.SaveAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Fail To Update User!");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file) // Return PhotoDto to get some data back for user
        {
            var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

            var result = await this.photoService.AddPhotoAsync(file);

            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }

            user.Photos.Add(photo);

            if (await this.userRepository.SaveAllAsync())
            {
                // one of the methods to return status code 201 with proper header

                return CreatedAtRoute("GetUser", new {username = user.UserName}, this.mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding Photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);

            if (currentMain != null) currentMain.IsMain = false;

            photo.IsMain = true;

            if (await this.userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Fail to Set Main Photo!");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await this.userRepository.GetUserByUsernameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(p => p.Id == photoId);

            if (photo == null) return NotFound();

            if (photo.IsMain) return BadRequest("You Cannot Delete Your Main Photo!");

            if (photo.PublicId != null)
            {
                var result = await this.photoService.DeletePhotoAsync(photo.PublicId);

                if (result.Error != null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);

            if (await this.userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed To Delete Photo!");
        }
    }

}