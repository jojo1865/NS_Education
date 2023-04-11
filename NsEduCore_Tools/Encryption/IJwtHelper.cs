using System.Collections.Generic;
using System.Security.Claims;

namespace NsEduCore_Tools.Encryption
{
    public interface IJwtHelper
    {
        /// <summary>
        /// 依據輸入的參數，產生 JWT Token。
        /// </summary>
        /// <param name="secretKey">JWT 密鑰</param>
        /// <param name="expireMinutes">有效時間（分鐘）</param>
        /// <param name="claims">Payload 參數</param>
        /// <returns>JWT Token 字串</returns>
        string GenerateToken(string secretKey, int expireMinutes, IEnumerable<Claim> claims);
    }
}