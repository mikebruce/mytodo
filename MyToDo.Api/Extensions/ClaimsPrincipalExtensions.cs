using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyToDo.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? user.FindFirst("nameid")?.Value
                     ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return int.TryParse(value, out var id) ? id : 0;
        }

        public static string? GetAccount(this ClaimsPrincipal user)
            => user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        public static bool IsAdmin(this ClaimsPrincipal user)
            => user.IsInRole("Admin");
    }
}