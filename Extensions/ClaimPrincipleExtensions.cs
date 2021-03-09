using System.Security.Claims;

namespace Extensions
{
    public static class ClaimPrincipleExtensions
    {
        // Get username from token
        public static string GetUsername(this ClaimsPrincipal user)
        {
            // the "Name" here represent UniqueName in TokenService when create new Claim
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        }
    }
}