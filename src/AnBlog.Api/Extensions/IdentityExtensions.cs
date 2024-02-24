using AnBlog.Core.SeedWorks.Constants;
using System.Security.Claims;

namespace AnBlog.Api.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetSpecificClaim(this ClaimsIdentity claimsIdentity, string claimType)
        {
            var claim = claimsIdentity.Claims.FirstOrDefault(x => x.Type == claimType);

            return (claim != null) ? claim.Value : string.Empty;
        }

        public static Guid GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            var userId = ((ClaimsIdentity) claimsPrincipal.Identity!).GetSpecificClaim(UserClaims.Id);

            return Guid.Parse(userId);
        }

    }
}
