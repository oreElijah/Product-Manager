using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace api.Extensions
{
    public static class ClaimsExtension
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            var username = user.FindFirst(JwtRegisteredClaimNames.Name)?.Value
            ?? user.FindFirst(ClaimTypes.Name)?.Value
            ?? user.FindFirst(ClaimTypes.GivenName)?.Value;

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new UnauthorizedAccessException("Username claim is missing from token.");
            }

            return username;
        }

        public static string GetJti(this ClaimsPrincipal user)
        {
            var jti = user.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
            ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(jti))
            {
                throw new UnauthorizedAccessException("JTI claim is missing from token.");
            }

            return jti;
        }

        public static string GetEmail(this ClaimsPrincipal user)
        {
            var email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? user.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new UnauthorizedAccessException("Email claim is missing from token.");
            }

            return email;
        }
    }
}