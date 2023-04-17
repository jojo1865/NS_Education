using System.Security.Claims;
using System.Web.Mvc;
using NS_Education.Variables;

namespace NS_Education.Tools.Filters
{
    public static class FilterStaticTools
    {
        public static string GetContextUri(ControllerContext context)
        {
            return context.HttpContext.Request.Url?.AbsoluteUri ?? "";
        }

        public static string GetUidInClaim(ClaimsPrincipal claims)
        {
            return claims.FindFirst(JwtConstants.UidClaimType)?.Value.Trim();
        }
        public static int GetUidInClaimInt(ClaimsPrincipal claims)
        {
            return int.Parse(GetUidInClaim(claims));
        }
    }
}