using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using NS_Education.Tools.Encryption;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Variables;

namespace NS_Education.Tools.Filters
{
    public static class FilterStaticTools
    {
        public static string GetContextUri(ControllerContext context)
        {
            return context.HttpContext.Request.Url?.AbsoluteUri ?? "";
        }

        /// <summary>
        /// 從 Request Header 中取得 JwtToken。
        /// </summary>
        /// <param name="request">Request</param>
        /// <returns>
        /// 具 Authorization header 時：Jwt 的字串。<br/>
        /// 否則：null。
        /// </returns>
        public static string GetJwtToken(HttpRequestBase request)
        {
            // JWT Bearer 方式可以避免 CSRF 攻擊。但是，如果這裡有任何形式的自動處理行為（如配合登入端點，自動把 JWT 讀/寫到 Cookie），
            // 就會失去這樣的保護作用。
            // 所以，在修改這裡的取值機制前，請先確認是否會破壞這樣的保護機制。

            string token = null;

            string header = request.Headers["Authorization"];

            if (header.HasContent() && header.StartsWith("Bearer "))
                token = request.Headers["Authorization"].Substring(7);

            return token;
        }

        public static bool HasRoleInRequest(HttpRequestBase request, AuthorizeBy role)
        {
            return AuthorizeTypeSingletonFactory
                .GetByType(role)
                .IsRoleInClaim(JwtHelper.DecodeToken(GetJwtToken(request), JwtConstants.Secret));
        }

        public static string GetUidInClaim(ClaimsPrincipal claims)
        {
            return claims.FindFirst(JwtConstants.UidClaimType)?.Value.Trim();
        }

        public static int GetUidInClaimInt(ClaimsPrincipal claims)
        {
            return int.Parse(GetUidInClaim(claims));
        }

        public static int GetUidInRequestInt(HttpRequestBase request)
        {
            return GetUidInClaimInt(JwtHelper.DecodeToken(GetJwtToken(request), JwtConstants.Secret));
        }

        public static string GetFieldInRequest(HttpRequestBase request, string fieldName)
        {
            return fieldName == null ? null : request[fieldName];
        }
    }
}