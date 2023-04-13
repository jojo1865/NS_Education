using System.Security.Claims;

namespace NS_Education.Tools.Filters.JwtAuthFilter.AuthorizeType
{
    public class AdminRole : IAuthorizeType
    {
        public bool IsRoleInClaim(ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.IsInRole(GetRoleValue());

        public string GetRoleValue()
            => RoleConstants.Admin;
    }
}