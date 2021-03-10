using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs;
using Entities;
using Helpers;

namespace Interfaces
{
    public interface ILikesRepository
    {
        // Get specific user like, individual like
        Task<UserLike> GetUserLike(int sourceId, int likedUserId);

        Task<AppUser> GetUserWithLikes(int userId);

        // Looking for a list of users that have been like and like back
        // Get from specific user
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams);
    }
}