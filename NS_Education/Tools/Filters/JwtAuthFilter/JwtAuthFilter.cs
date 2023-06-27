using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.Entities;
using NS_Education.Tools.Encryption;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter.AuthorizeType;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;
using static System.Data.Entity.QueryableExtensions;

namespace NS_Education.Tools.Filters.JwtAuthFilter
{
    public class JwtAuthFilter : ActionFilterAttribute
    {
        private readonly string _addOrEditKeyFieldName;
        private readonly bool _ignorePasswordExpired;
        private readonly RequiredPrivileges _privileges;

        private readonly IAuthorizeType[] _roles;
        private readonly string _uidFieldName;

        /// <summary>
        /// 套用 JWT 驗證，並且需符合指定的 Roles。<br/>
        /// 包含 UserSelf 時，會針對 Request JSON 中的欄位比對是否與 JWT Payload 相符。<br/>
        /// 預設找的欄位名稱請參照「<see cref="IoConstants.IdFieldName"/>」。
        /// </summary>
        /// <param name="roles">允許的 roles。（可選）忽略時，不驗證 Roles。</param>
        /// <param name="privileges">所需的群組 Flag。（可選）忽略時，不驗證群組 Flag。</param>
        /// <param name="ignorePasswordExpired">是否允許忽略檢查密碼過期與否。</param>
        /// <exception cref="ArgumentException">當 RequirePrivilege 指定 AddOrEdit 時拋錯。不應使用此建構式。</exception>
        public JwtAuthFilter(AuthorizeBy roles
            , RequirePrivilege privileges
            , bool ignorePasswordExpired)
        {
            if (privileges.HasFlag(RequirePrivilege.AddOrEdit))
                throw new ArgumentException(RequirePrivilegeAddOrEditNoFieldName);

            _roles = AuthorizeTypeSingletonFactory.GetEnumerableByEnum(roles).ToArray();
            _privileges = new RequiredPrivileges(privileges);
            _uidFieldName = IoConstants.IdFieldName;
            _addOrEditKeyFieldName = null;
            _ignorePasswordExpired = ignorePasswordExpired;
        }

        /// <summary>
        /// 套用 JWT 驗證，並且需符合指定的 Roles。<br/>
        /// 包含 UserSelf 時，會針對 Request JSON 中的欄位比對是否與 JWT Payload 相符。<br/>
        /// 預設找的欄位名稱請參照「<see cref="IoConstants.IdFieldName"/>」。
        /// </summary>
        /// <param name="roles">允許的 roles。</param>
        /// <param name="privileges">所需的群組 Flag。</param>
        /// <exception cref="ArgumentException">當 RequirePrivilege 指定 AddOrEdit 時拋錯。不應使用此建構式。</exception>
        public JwtAuthFilter(AuthorizeBy roles
            , RequirePrivilege privileges)
        {
            if (privileges.HasFlag(RequirePrivilege.AddOrEdit))
                throw new ArgumentException(RequirePrivilegeAddOrEditNoFieldName);

            _roles = AuthorizeTypeSingletonFactory.GetEnumerableByEnum(roles).ToArray();
            _privileges = new RequiredPrivileges(privileges);
            _uidFieldName = IoConstants.IdFieldName;
            _addOrEditKeyFieldName = null;
        }

        /// <summary>
        /// 套用 JWT 驗證，並且需符合指定的 Roles。<br/>
        /// 包含 UserSelf 時，會針對 Request JSON 中的欄位比對是否與 JWT Payload 相符。<br/>
        /// 可以透過 uidFieldName 指定欄位名稱。
        /// </summary>
        /// <param name="roles">允許的 roles。</param>
        /// <param name="privileges">所需的群組 Flag。</param>
        /// <param name="uidFieldName">Request JSON 中的 UID 欄位名稱。null 時，代入「<see cref="IoConstants.IdFieldName"/>」。</param>
        /// <param name="addOrEditKeyFieldName">Request JSON 中，用於判定需要新增還是修改權限的欄位名稱。通常對應某種 ID 欄位，值為 0 時視為新增。</param>
        /// <exception cref="ArgumentException">當 RequirePrivilege 指定 AddOrEdit 卻未提供 addOrEditKeyFieldName 時拋錯。不應使用此建構式。</exception>
        public JwtAuthFilter(AuthorizeBy roles
            , RequirePrivilege privileges
            , string uidFieldName
            , string addOrEditKeyFieldName)
        {
            if (privileges.HasFlag(RequirePrivilege.AddOrEdit) && addOrEditKeyFieldName.IsNullOrWhiteSpace())
                throw new ArgumentException(RequirePrivilegeAddOrEditNoFieldName);

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
            ICollection<B_StaticCode> safetyConfiguration;

            // 先查安全控管設定到本地
            using (NsDbContext nsDbContext = new NsDbContext())
            {
                safetyConfiguration = nsDbContext.B_StaticCode
                    .Where(sc => sc.CodeType == (int)StaticCodeType.SafetyControl && sc.ActiveFlag && !sc.DeleteFlag)
                    .ToArray();
            }

            // 1. 驗證有 Token 且解析正常無誤。
            // 2. 當設定檔要求時，驗證 Token 符合對應 UserData 的最新 Token。
            // 3. 驗證 Token 中 Claim 包含指定的 Role。
            // 4. 驗證 Privilege，所有 Flag 在任一所屬 Group 均有允許。

            bool isValid = actionContext.StartValidate()
                .SkipIfAlreadyInvalid()
                .Validate(c => ValidateTokenDecryptable(c, JwtConstants.Secret, out claims),
                    (_, e) => errorMessage = HasValidTokenFailed(e))
                .Validate(c => ValidateTokenIsLatest(c, claims, safetyConfiguration),
                    (_, e) => errorMessage = e.Message)
                .Validate(c => ValidateLastPasswordChange(claims, safetyConfiguration),
                    e => throw e) // 只有這裡要回傳 901，所以不做 catch
                .Validate(c => ValidateClaimRole(c, claims),
                    _ => errorMessage = HasNoRoleOrPrivilege)
                .Validate(c => ValidatePrivileges(c, claims),
                    _ => errorMessage = HasNoRoleOrPrivilege)
                .IsValid();

            if (isValid) return;

            throw new HttpException((int)HttpStatusCode.Unauthorized,
                $"JWT 驗證失敗。{errorMessage}".SanitizeForResponseStatusMessage());
        }

        private void ValidateLastPasswordChange(ClaimsPrincipal claims, ICollection<B_StaticCode> safetyConfig)
        {
            // 如果這個 JwtAuthFilter 設置為忽略此設定，提早折回。
            if (_ignorePasswordExpired)
                return;

            // 先檢查設定檔，如果值小於等於 0，不做任何驗證。
            int forceChangeDays = GetSafetyConfigValue(safetyConfig, StaticCodeSafetyControlCode.PasswordExpireDays);

            if (forceChangeDays <= 0)
                return;

            // 驗證這個使用者距離最後一次修改密碼的天數
            int uid = FilterStaticTools.GetUidInClaimInt(claims);

            using (NsDbContext nsDbContext = new NsDbContext())
            {
                // 作為預設值，取得帳號建立日期
                DateTime lastChangeTime = nsDbContext.UserData
                    .Where(ud => ud.ActiveFlag && !ud.DeleteFlag)
                    .Where(ud => ud.UID == uid)
                    .Select(ud => ud.CreDate)
                    .FirstOrDefault();

                // 查詢變更密碼紀錄
                UserPasswordLog log = nsDbContext.UserPasswordLog
                    .Where(upl => upl.UID == uid)
                    .Where(upl => upl.Type == (int)UserPasswordLogType.ChangePassword)
                    .OrderByDescending(upl => upl.CreDate)
                    .FirstOrDefault();

                // 如果有任何變更密碼紀錄，才以變更密碼紀錄的日期為準
                if (log != null)
                    lastChangeTime = log.CreDate;

                // 參數名是「有效天數」，假設值為 60，而相減正好是第 60 天，先仍視為有效
                if ((DateTime.Today - lastChangeTime.Date).Days <= forceChangeDays)
                    return;

                // 回傳 901
                throw new HttpException((int)CustomHttpCode.PasswordExpired, "密碼已過期！");
            }
        }

        private void ValidateTokenIsLatest(ActionExecutingContext actionExecutingContext, ClaimsPrincipal claims,
            IEnumerable<B_StaticCode> safetyConfig)
        {
            // 先檢查設定檔，如果此功能關閉，就不做任何驗證。
            bool isEnabled =
                GetSafetyConfigValue(safetyConfig, StaticCodeSafetyControlCode.EnforceOneSessionPerUser) == 1;

            if (!isEnabled)
                return;

            // 驗證 Token 符合 UserData 中紀錄的 JWT
            int uid = FilterStaticTools.GetUidInClaimInt(claims);

            using (NsDbContext nsDbContext = new NsDbContext())
            {
                UserData user =
                    nsDbContext.UserData.FirstOrDefault(ud => ud.UID == uid && ud.ActiveFlag && !ud.DeleteFlag);

                if (user is null)
                    throw new Exception(UserDataNotFound);

                if (user.JWT != GetToken(actionExecutingContext))
                    throw new Exception(TokenExpired);
            }
        }

        private static int GetSafetyConfigValue(IEnumerable<B_StaticCode> safetyConfig,
            StaticCodeSafetyControlCode code)
        {
            return safetyConfig
                .Where(sc => sc.Code == ((int)code).ToString())
                .Select(sc => sc.SortNo)
                .FirstOrDefault();
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
            using (NsDbContext nsDbContext = new NsDbContext())
            {
                string contextUri = FilterStaticTools.GetContextUri(actionContext);

                var query = nsDbContext.UserData
                        .Include(u => u.M_Group_User)
                        .Include(u => u.M_Group_User.Select(mgu => mgu.GroupData))
                        .Include(u => u.M_Group_User.Select(mgu => mgu.GroupData).Select(g => g.M_Group_Menu))
                        .Include(u =>
                            u.M_Group_User.Select(mgu => mgu.GroupData)
                                .Select(g => g.M_Group_Menu.Select(mgm => mgm.MenuData)))
                        .Include(u => u.M_Group_User.Select(mgu => mgu.GroupData).Select(g =>
                            g.M_Group_Menu.Select(mgm => mgm.MenuData).Select(md => md.MenuAPI)))
                        .Where(u => u.UID == uid && u.ActiveFlag && !u.DeleteFlag)
                        .SelectMany(u => u.M_Group_User)
                        .Select(groupUser => groupUser.GroupData)
                        .Where(group => group.ActiveFlag && !group.DeleteFlag)
                        .SelectMany(group => group.M_Group_Menu)
                        .Where(groupMenu => groupMenu.MenuData.ActiveFlag
                                            && !groupMenu.MenuData.DeleteFlag)
                        .Where(groupMenu => groupMenu.MenuData.MenuAPI.Any(api =>
                            contextUri.Contains(api.APIURL) || api.APIURL == PrivilegeConstants.RootAccessUrl))
                    ;

                // 4. 具備所有所需 Flags 時，才回傳 true。
                return HasAllFlagsInDb(actionContext, query.ToList());
            }
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
                showFlagOk |= gm.ShowFlag || gm.MenuData.AlwaysAllowShow;
                addFlagOk |= gm.AddFlag || gm.MenuData.AlwaysAllowAdd;
                editFlagOk |= gm.EditFlag || gm.MenuData.AlwaysAllowEdit;
                deleteFlagOk |= gm.DeleteFlag || gm.MenuData.AlwaysAllowDelete;
                printFlagOk |= gm.PringFlag || gm.MenuData.AlwaysAllowPring;

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
            string uidInRequest =
                FilterStaticTools.GetFieldInRequest(context.HttpContext.Request, _uidFieldName)?.Trim();
            string uidInClaim = FilterStaticTools.GetUidInClaim(claims);

            return String.Equals(uidInClaim, uidInRequest);
        }

        public static void ValidateTokenDecryptable(ControllerContext actionContext, string secret,
            out ClaimsPrincipal claims)
        {
            claims = null;
            string token;

            // 1. 驗證是否拿得到 token
            try
            {
                token = GetToken(actionContext);
                if (token.IsNullOrWhiteSpace())
                    throw new Exception();
            }
            catch (Exception)
            {
                throw new HttpRequestValidationException(RequestHeaderLacksAuthorization);
            }

            // 2. 驗證 Token 是否可正常解密
            try
            {
                claims = JwtHelper.DecodeToken(token, secret);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(DecodeTokenFailed(e));
            }
        }

        private static string GetToken(ControllerContext actionContext)
        {
            return FilterStaticTools.GetJwtToken(actionContext.HttpContext.Request);
        }

        #region 錯誤訊息

        private static string DecodeTokenFailed(Exception e)
            => $"解密 JWT Token 失敗：{e.Message}！";

        private const string RequestHeaderLacksAuthorization = "從 Header 找不到 JWT！";

        private const string HasNoRoleOrPrivilege = "此 UID 無此權限！";

        private static string HasValidTokenFailed(Exception e)
            => $"驗證 JWT Token 時出錯：{e.Message}";

        private const string RequirePrivilegeAddOrEditNoFieldName =
            "RequirePrivilege 指定 AddOrEdit，卻沒有提供 addOrEditKeyFieldName！";

        private const string TokenExpired = "登入已過期，請重新登入！";
        private const string UserDataNotFound = "查無對應的 UID！";

        #endregion
    }
}