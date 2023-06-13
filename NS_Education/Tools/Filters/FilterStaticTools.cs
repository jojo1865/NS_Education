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

        public static string GetJwtToken(HttpRequestBase request)
        {
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