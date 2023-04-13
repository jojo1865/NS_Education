using System;
using System.Security.Claims;

namespace NS_Education.Tools.Filters.JwtAuthFilter.AuthorizeType
{
    public class AnyRole : IAuthorizeType
    {
        public bool IsRoleInClaim(ClaimsPrincipal claimsPrincipal)
            => true;

        public string GetRoleValue()
            => throw new NotSupportedException("AnyRole doesn't have role name.");
    }
}