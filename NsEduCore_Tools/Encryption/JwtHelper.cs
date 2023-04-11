using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using NsEduCore_Tools.Extensions;

namespace NsEduCore_Tools.Encryption
{
    public class JwtHelper : IJwtHelper
    {
        public string GenerateToken(string secretKey, int expireMinutes, IEnumerable<Claim> claims)
        {
            if (secretKey.IsNullOrWhitespace())
                throw new ArgumentNullException(nameof(secretKey));
            
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                SigningCredentials = signingCredentials,
                Subject = new ClaimsIdentity(claims)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}