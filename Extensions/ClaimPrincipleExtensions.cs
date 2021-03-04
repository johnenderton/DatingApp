using System.Security.Claims;

namespace Extensions
{
    public static class ClaimPrincipleExtensions
    {
        // Get username from token
        public static string GetUsername(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
    }
}