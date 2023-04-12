using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace NS_Education.Tools.Encryption
{
    public static class JwtHelper
    {
        private static JwtSecurityTokenHandler TokenHandler { get; } = new JwtSecurityTokenHandler(); 
        
        /// <summary>
        /// 依據輸入的參數，產生 JWT Token。
        /// </summary>
        /// <param name="secretKey">JWT 密鑰</param>
        /// <param name="expireMinutes">有效時間（分鐘）</param>
        /// <param name="claims">Payload 參數</param>
        /// <returns>JWT Token 字串</returns>
        public static string GenerateToken(string secretKey, int expireMinutes, IEnumerable<Claim> claims)
        {
            var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
                SigningCredentials = signingCredentials,
                Subject = new ClaimsIdentity(claims)
            };
            
            var token = TokenHandler.CreateToken(tokenDescriptor);
            return TokenHandler.WriteToken(token);
        }

        /// <summary>
        /// 驗證一組輸入的 JWT Token。
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <param name="secretKey">JWT 密鑰</param>
        /// <returns>
        /// true：解密成功。<br/>
        /// false：解密失敗，或是過程拋錯時。
        /// </returns>
        public static bool ValidateToken(string token, string secretKey)
        {
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true, // JWT 在加密時會自動把時間設為 exp 值，所以這裡不需要再提供有效時間為幾分鐘。
                    ClockSkew = TimeSpan.Zero
                };

                TokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// 解密一組輸入的 JWT Token。失敗時不做 try-catch。
        /// </summary>
        /// <param name="token">JWT Token</param>
        /// <param name="secretKey">JWT 密鑰</param>
        /// <returns>ClaimsPrincipal。</returns>
        public static ClaimsPrincipal DecodeToken(string token, string secretKey)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true, // JWT 在加密時會自動把時間設為 exp 值，所以這裡不需要再提供有效時間為幾分鐘。
                ClockSkew = TimeSpan.Zero
            };

            return TokenHandler.ValidateToken(token, validationParameters, out _);
        }
    }
}