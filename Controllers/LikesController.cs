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
        private readonly IUnitOfWork unitOfWork;
        public LikesController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username) // perform action: like an user
        {
            var sourceUserId = User.GetUserId();
            var likedUser = await this.unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var sourceUser = await this.unitOfWork.LikesRepository.GetUserWithLikes(sourceUserId);

            if (likedUser == null) return NotFound(); // Check if this user exist

            if (sourceUser.UserName == username) return BadRequest("You Cannot Like Yourself!");

            var userLike = await this.unitOfWork.LikesRepository.GetUserLike(sourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("You Already Liked This User!");

            // Create New Record
            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                LikedUserId = likedUser.Id
            };
            sourceUser.LikedUsers.Add(userLike); // Add to Collection

            if (await this.unitOfWork.Complete()) return Ok();

            return BadRequest("Failed To Like User!");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await this.unitOfWork.LikesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            // Cannot return directly, ActionResult does not work well with IEnumerable
            return Ok(users);
        }
    }
}