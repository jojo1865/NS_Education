using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.Entities;
using NS_Education.Models.Entities.DbContext;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.Encryption;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter.AuthorizeType;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Tools.Filters.JwtAuthFilter
{
    public class JwtAuthFilter : ActionFilterAttribute
    {
        #region 錯誤訊息

        private static string DecodeTokenFailed(Exception e)
            => $"解密 JWT Token 失敗：{e.Message}！";
        private const string RequestHeaderLacksAuthorization = "HTTP Header 未找到 Authorization 欄位，或是 Bearer 格式不正確！";

        private const string HasNoRoleOrPrivilege = "此 UID 無此權限";
        private static string HasValidTokenFailed(Exception e)
            => $"驗證 JWT Token 時出錯：{e.Message}";

        #endregion

        private readonly IAuthorizeType[] _roles;
        private readonly RequiredPrivileges _privileges;
        private readonly string _uidFieldName;
        private readonly string _addOrEditKeyFieldName;

        /// <summary>
        /// 套用 JWT 驗證，並且需符合指定的 Roles。<br/>
        /// 包含 UserSelf 時，會針對 Request JSON 中的欄位比對是否與 JWT Payload 相符。<br/>
        /// 可以透過 uidFieldName 指定欄位名稱。
        /// </summary>
        /// <param name="roles">允許的 roles。（可選）忽略時，不驗證 Roles。</param>
        /// <param name="privileges">所需的群組 Flag。（可選）忽略時，不驗證群組 Flag。</param>
        /// <param name="uidFieldName">Request JSON 中的 UID 欄位名稱。（可選）預設值為「<see cref="IoConstants.IdFieldName"/>」。</param>
        /// <param name="addOrEditKeyFieldName">Request JSON 中，用於判定需要新增還是修改權限的欄位名稱。通常對應某種 ID 欄位，值為 0 時視為新增。（可選）忽略時，在啟動時報錯。</param>
        // ReSharper 可能會建議 roles 改用 IEnumerable, 但 C# Attribute 並不支援該類型的 constructor argument。
        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        public JwtAuthFilter(AuthorizeBy roles
            , RequirePrivilege privileges
            , string uidFieldName
            , string addOrEditKeyFieldName)
        {
            if (privileges.HasFlag(RequirePrivilege.AddOrEdit) && addOrEditKeyFieldName.IsNullOrWhiteSpace())
                throw new ArgumentException("RequirePrivilege 指定 AddOrEdit，卻沒有提供 addOrEditKeyFieldName！");
                    
            _roles = AuthorizeTypeSingletonFactory.GetEnumerableByEnum(roles).ToArray();
            _privileges = new RequiredPrivileges(privileges);
            _uidFieldName = uidFieldName ?? IoConstants.IdFieldName;
            _addOrEditKeyFieldName = addOrEditKeyFieldName;
            
        }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            JwtAuth(actionContext);
            base.OnActionExecuting(actionContext);
        }

        private void JwtAuth(ActionExecutingContext actionContext)
        {
            string errorMessage = null;
            ClaimsPrincipal claims = null;

            // 1. 驗證有 Token 且解析正常無誤。
            // 2. 驗證 Token 中 Claim 包含指定的 Role。
            // 3. 驗證 Privilege，所有 Flag 在任一所屬 Group 均有允許。
            bool isValid = actionContext.StartValidate(true)
                .Validate(c => ValidateToken(c, JwtConstants.Secret, out claims),
                    e => errorMessage = HasValidTokenFailed(e))
                .Validate(c => ValidateClaimRole(c, claims),
                    () => errorMessage = HasNoRoleOrPrivilege)
                .Validate(c => ValidatePrivileges(c, claims),
                    () => errorMessage = HasNoRoleOrPrivilege)
                .IsValid();

            if (!isValid)
                actionContext.Result = new HttpUnauthorizedResult($"JWT 驗證失敗。{errorMessage}".SanitizeForResponseStatusMessage());
        }

        private bool ValidatePrivileges(ActionExecutingContext actionContext, ClaimsPrincipal claims)
        {
            // 1. 若無權限限制，提早返回。
            if (_privileges.None)
                return true;

            // 2. 從 claims 取得 UID，無 UID 時提早返回。
            if (!int.TryParse(FilterStaticTools.GetUidInClaim(claims), out int uid))
                return false;

            // 3. 依據 uid 查詢所有權限。
            // User -> M_Group_User -> GroupData -> M_Group_Menu -> MenuData -> MenuAPI
            NsDbContext context = new NsDbContext();
            var query = context.UserData
                    .Include(u => u.M_Group_User)
                    .ThenInclude(groupUser => groupUser.G)
                    .ThenInclude(group => group.M_Group_Menu)
                    .ThenInclude(groupMenu => groupMenu.MD)
                    .ThenInclude(menuData => menuData.MenuAPI)
                    .Where(u => u.UID == uid && u.ActiveFlag && !u.DeleteFlag)
                    .SelectMany(u => u.M_Group_User)
                    .Select(groupUser => groupUser.G)
                    .SelectMany(group => group.M_Group_Menu)
                    .Where(groupMenu => groupMenu.G.ActiveFlag
                                        && !groupMenu.G.DeleteFlag
                                        && groupMenu.MD.ActiveFlag
                                        && !groupMenu.MD.DeleteFlag
                                        && FilterStaticTools.GetContextUri(actionContext).Contains(groupMenu.MD.URL)
                    )
                ;

            // 4. 具備所有所需 Flags 時，才回傳 true。
            return HasAllFlagsInDb(actionContext, query.ToList());
        }

        private bool HasAllFlagsInDb(ActionExecutingContext actionContext, List<M_Group_Menu> queried)
        {
            bool showFlagOk = !_privileges.RequireShowFlag;
            bool addFlagOk = !_privileges.RequireAddFlag; 
            bool editFlagOk = !_privileges.RequireEditFlag;
            bool deleteFlagOk = !_privileges.RequireDeleteFlag;
            bool printFlagOk = !_privileges.RequirePrintFlag;
            
            // 當有 AddOrEditFlag 時, 特別處理 addFlag/editFlag 和 request 欄位
            if (_privileges.RequireAddOrEditFlag)
            {
                addFlagOk = addFlagOk && !AddOrEditNeedsAddFlag(actionContext);
                editFlagOk = editFlagOk && !AddOrEditNeedsEditFlag(actionContext);
            }

            foreach (M_Group_Menu gm in queried)
            {
                showFlagOk |= gm.ShowFlag;
                addFlagOk |= gm.AddFlag;
                editFlagOk |= gm.EditFlag;
                deleteFlagOk |= gm.DeleteFlag;
                printFlagOk |= gm.PringFlag;

                if (showFlagOk && addFlagOk && editFlagOk && deleteFlagOk && printFlagOk)
                    return true;
            }

            return false;
        }

        private bool AddOrEditNeedsAddFlag(ActionExecutingContext actionContext)
        {
            return FilterStaticTools
                .GetFieldInRequest(actionContext.HttpContext.Request, _addOrEditKeyFieldName)?
                .Trim() 
                   == IoConstants.IdValueWhenSubmit;
        }

        private bool AddOrEditNeedsEditFlag(ActionExecutingContext actionContext)
        {
            string fieldValue = FilterStaticTools
                .GetFieldInRequest(actionContext.HttpContext.Request, _addOrEditKeyFieldName)?
                .Trim();
            return fieldValue != null
                   && fieldValue != IoConstants.IdValueWhenSubmit;
        }

        private bool ValidateClaimRole(ControllerContext context, ClaimsPrincipal claims)
        {
            return _roles.Any(t => IsRoleValid(context, claims, t));
        }

        private bool IsRoleValid(ControllerContext context, ClaimsPrincipal claims, IAuthorizeType t)
        {
            bool isValid = t is AnyRole || t.IsRoleInClaim(claims);

            // UserSelf 的情況，在前述驗證都通過時，需要與 Request JSON 的欄位再做驗證
            // 這比較像是 JwtAuthFilter 的特殊規格，而非 AuthorizeType 應該提供的功能
            // 所以寫在這個 class
            if (isValid && t is UserRole)
                isValid = ValidateUserIdInRequest(context, claims);

            return isValid;
        }

        /// <summary>
        /// 對照 Request JSON 中的 ID 欄位和 JWT Claim 中的 UID
        /// </summary>
        /// <param name="context"></param>
        /// <param name="claims"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool ValidateUserIdInRequest(ControllerContext context, ClaimsPrincipal claims)
        {
            string uidInRequest = FilterStaticTools.GetFieldInRequest(context.HttpContext.Request, _uidFieldName)?.Trim();
            string uidInClaim = FilterStaticTools.GetUidInClaim(claims);

            return String.Equals(uidInClaim, uidInRequest);
        }

        public static void ValidateToken(ControllerContext actionContext, string secret, out ClaimsPrincipal claims)
        {
            claims = null;

            // 1. 驗證 Header 中是否有 Authorization
            if (!HasBearerAuthorization(actionContext))
                throw new HttpRequestValidationException(RequestHeaderLacksAuthorization);

            // 2. 驗證 Token 是否可正常解密
            try
            {
                claims = JwtHelper.DecodeToken(GetToken(actionContext), secret);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(DecodeTokenFailed(e));
            }
        }

        private static string GetToken(ControllerContext actionContext)
        {
            return FilterStaticTools.GetJwtTokenInHeader(actionContext.HttpContext.Request);
        }

        private static bool HasBearerAuthorization(ControllerContext actionContext)
        {
            return actionContext.HttpContext.Request.Headers["Authorization"]?.StartsWith("Bearer") ?? false;
        }
    }
}