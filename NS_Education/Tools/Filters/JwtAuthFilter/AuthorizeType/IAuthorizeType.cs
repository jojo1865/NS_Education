using System.Security.Claims;

namespace NS_Education.Tools.Filters.JwtAuthFilter.AuthorizeType
{
    public interface IAuthorizeType
    {
        /// <summary>
        /// 驗證輸入的 Claims 符不符合這個 Role 的條件。
        /// </summary>
        /// <param name="claimsPrincipal">Claims</param>
        /// <returns>
        /// true：符合。<br/>
        /// false：不符合。
        /// </returns>
        bool IsRoleInClaim(ClaimsPrincipal claimsPrincipal);

        /// <summary>
        /// 取得這個驗證種類對應的 Role 值。
        /// </summary>
        /// <returns>Role 名稱的字串。</returns>
        string GetRoleValue();
    }
}