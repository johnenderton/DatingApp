using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs;
using Entities;
using Extensions;
using Helpers;
using Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository userRepository;
        private readonly ILikesRepository likesRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            this.likesRepository = likesRepository;
            this.userRepository = userRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username) // perform action: like an user
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await this.userRepository.GetUserByUsernameAsync(username);
            var sourceUser = await this.likesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound(); // Check if this user exist

            if (sourceUser.UserName == username) return BadRequest("You Cannot Like Yourself!");

            var userLike = await this.likesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("You Already Liked This User!");

            // Create New Record
            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };
            sourceUser.LikedUsers.Add(userLike); // Add to Collection

            if (await this.userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed To Like User!");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await this.likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            // Cannot return directly, ActionResult does not work well with IEnumerable
            return Ok(users);
        }
    }
}