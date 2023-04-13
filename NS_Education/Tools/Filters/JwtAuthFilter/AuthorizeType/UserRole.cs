using System.Security.Claims;

namespace NS_Education.Tools.Filters.JwtAuthFilter.AuthorizeType
{
    public class UserRole : IAuthorizeType
    {
        public bool IsRoleInClaim(ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.IsInRole(GetRoleValue());

        public string GetRoleValue()
            => RoleConstants.User;
    }
}