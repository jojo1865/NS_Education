﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using Microsoft.IdentityModel.Tokens;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.UserData.UserData.AdminAuthorize;
using NS_Education.Models.APIItems.Controller.UserData.UserData.AdminChangeUserPassword;
using NS_Education.Models.APIItems.Controller.UserData.UserData.BatchSubmitDepartment;
using NS_Education.Models.APIItems.Controller.UserData.UserData.BatchSubmitGroup;
using NS_Education.Models.APIItems.Controller.UserData.UserData.GetInfoById;
using NS_Education.Models.APIItems.Controller.UserData.UserData.GetList;
using NS_Education.Models.APIItems.Controller.UserData.UserData.IsAdministrator;
using NS_Education.Models.APIItems.Controller.UserData.UserData.Login;
using NS_Education.Models.APIItems.Controller.UserData.UserData.Submit;
using NS_Education.Models.APIItems.Controller.UserData.UserData.UpdatePW;
using NS_Education.Models.Entities;
using NS_Education.Models.Errors;
using NS_Education.Models.Errors.AuthorizationErrors;
using NS_Education.Tools.ControllerTools.BaseClass;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper;
using NS_Education.Tools.ControllerTools.BasicFunctions.Helper.Interface;
using NS_Education.Tools.ControllerTools.BasicFunctions.Interface;
using NS_Education.Tools.Encryption;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controller.UsingHelper.UserDataController
{
    public class UserDataController : PublicClass,
        IGetListPaged<UserData, UserData_GetList_Input_APIItem, UserData_GetList_Output_Row_APIItem>,
        IGetInfoById<UserData, UserData_GetInfoById_Output_APIItem>,
        IDeleteItem<UserData>,
        ISubmit<UserData, UserData_Submit_Input_APIItem>,
        IChangeActive<UserData>
    {
        #region 錯誤訊息 - 註冊/更新

        private const string PasswordAlphanumericOnly = "使用者密碼只允許半形英文字母、數字！";

        #endregion

        #region 錯誤訊息 - 登入

        private const string LoginPasswordIncorrect = "使用者密碼錯誤！";

        #endregion

        #region AdminAuthorize

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.None)]
        public async Task<string> AdminAuthorize(UserData_AdminAuthorize_Input_APIItem input)
        {
            int uid = GetUid();
            UserData data = await DC.UserData.FirstOrDefaultAsync(ud => ud.UID == uid);

            if (data is null)
            {
                AddError(NotFound("使用者 ID", "UID"));
                return GetResponseJson();
            }

            bool isValid = ValidatePassword(input.Password, data.LoginPassword);

            if (!isValid)
                AddError(new WrongPasswordError());

            return GetResponseJson();
        }

        #endregion

        #region AdminChangeUserPassword

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> AdminChangeUserPassword(UserData_AdminChangeUserPassword_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .ValidateAsync(async i => await DC.UserData.ValidateIdExists(input.UID, nameof(UserData.UID)),
                    i => AddError(NotFound("使用者", nameof(i.UID))))
                .Validate(i => i.Password.HasContent(),
                    () => AddError(EmptyNotAllowed("新的密碼", nameof(input.Password))))
                .Validate(i => i.Password.IsEncryptablePassword(),
                    () => AddError(WrongFormat("新的密碼", nameof(input.Password))))
                .IsValid();

            if (!isValid)
                return GetResponseJson();

            UserData userData = await DC.UserData.FirstAsync(ud => ud.UID == input.UID);

            await UpdatePasswordForUserData(userData, input.Password);

            await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);

            return GetResponseJson();
        }

        #endregion

        #region IsAdministrator

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.None)]
        public string IsAdministrator()
        {
            UserData_IsAdministrator_Output_APIItem result = new UserData_IsAdministrator_Output_APIItem
            {
                IsAdministrator = FilterStaticTools.HasRoleInRequest(Request, AuthorizeBy.Admin)
            };

            return GetResponseJson(result);
        }

        #endregion

        #region Validate department user count

        private async Task<bool> ValidateDepartmentCapacity(int departmentId, IEnumerable<int> userIds)
        {
            userIds = userIds.Distinct().ToHashSet();

            D_Department department = await DC.D_Department.Include(dd => dd.UserData)
                .Where(dd => !dd.DeleteFlag)
                .Where(dd => dd.ActiveFlag)
                .FirstOrDefaultAsync(dd => dd.DDID == departmentId);

            if (department is null)
                return false;

            return department.UserData
                       .Where(ud => !userIds.Contains(ud.UID))
                       .Count(ud => ud.ActiveFlag && !ud.DeleteFlag) + userIds.Count()
                   <= department.PeopleCt;
        }

        #endregion

        #region SignUp

        /// <summary>
        ///     註冊使用者資料。過程中會驗證使用者輸入，並在回傳時一併報錯。<br />
        ///     如果過程驗證都通過，才寫入資料庫。
        /// </summary>
        /// <param name="input">輸入資料</param>
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.AddFlag)]
        public async Task<string> SignUp(UserData_Submit_Input_APIItem input)
        {
            bool isValid = await SignUpValidateInput(input);

            if (!isValid)
                return GetResponseJson();

            UserData newUser = SignUpCreateUserData(input);
            await SignUpSaveToDb(newUser);

            return GetResponseJson();
        }

        private async Task SignUpSaveToDb(UserData newUser)
        {
            try
            {
                await DC.UserData.AddAsync(newUser);
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
            }
            catch (Exception e)
            {
                AddError(UpdateDbFailed(e));
            }
        }

        private static UserData SignUpCreateUserData(UserData_Submit_Input_APIItem input)
        {
            var newUser = new UserData
            {
                UserName = input.Username,
                LoginAccount = input.LoginAccount,
                LoginPassword = EncryptPassword(input.LoginPassword),
                Note = input.Note,
                LoginDate = DateTime.Now,
                DDID = input.DDID,
                M_Group_User = new List<M_Group_User>
                {
                    new M_Group_User
                    {
                        GID = input.GID
                    }
                },
                ActiveFlag = true
            };
            return newUser;
        }

        private async Task<bool> SignUpValidateInput(UserData_Submit_Input_APIItem input)
        {
            int passwordMinLength = GetPasswordMinLength();

            bool isValid = await input.StartValidate()
                .ValidateAsync(
                    async i => await DC.GroupData.ValidateIdExists(i.GID, nameof(GroupData.GID)),
                    () => AddError(NotFound("角色 ID", nameof(input.GID))))
                .ValidateAsync(
                    async i => await DC.D_Department.ValidateIdExists(i.DDID, nameof(D_Department.DDID)),
                    () => AddError(NotFound("部門 ID", nameof(input.DDID))))
                .ValidateAsync(async i => await ValidateDepartmentCapacity(i.DDID, new[] { input.UID }),
                    () => AddError(2, "指定的部門已達編制人數上限！"))
                .Validate(i => i.Username.HasContent() && i.Username.Length.IsInBetween(1, 50),
                    () => AddError(LengthOutOfRange("使用者名稱", nameof(input.Username), 1, 50)))
                .Validate(i => i.LoginAccount.HasContent() && i.LoginAccount.Length.IsInBetween(1, 100),
                    () => AddError(LengthOutOfRange("使用者帳號", nameof(input.LoginAccount), 1, 100)))
                // 驗證使用者帳號尚未被使用
                .ValidateAsync(
                    async i => !await DC.UserData.AnyAsync(ud =>
                        !ud.DeleteFlag && ud.LoginAccount == input.LoginAccount),
                    () => AddError(AlreadyExists("使用者帳號", nameof(input.LoginAccount))))
                .Validate(i => i.LoginPassword.HasContent(),
                    () => AddError(EmptyNotAllowed("使用者密碼", nameof(input.LoginPassword))))
                .SkipIfAlreadyInvalid()
                .Validate(i => i.LoginPassword.Length.IsInBetween(passwordMinLength, 100),
                    () => AddError(LengthOutOfRange("使用者密碼", nameof(input.LoginPassword), passwordMinLength, 100)))
                .Validate(i => i.LoginPassword.IsEncryptablePassword(), () => AddError(1, PasswordAlphanumericOnly))
                .IsValid();
            return isValid;
        }

        #endregion

        #region 初始化

        private readonly IGetListPagedHelper<UserData_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<UserData_Submit_Input_APIItem> _submitHelper;
        private readonly IChangeActiveHelper _changeActiveHelper;

        private readonly IGetInfoByIdHelper _getInfoByIdHelper;

        public UserDataController()
        {
            _getListPagedHelper = new
                GetListPagedHelper<UserDataController, UserData, UserData_GetList_Input_APIItem,
                    UserData_GetList_Output_Row_APIItem>(this);

            _deleteItemHelper = new
                DeleteItemHelper<UserDataController, UserData>(this);

            _submitHelper = new
                SubmitHelper<UserDataController, UserData, UserData_Submit_Input_APIItem>(this);

            _changeActiveHelper = new
                ChangeActiveHelper<UserDataController, UserData>(this);
            _getInfoByIdHelper =
                new GetInfoByIdHelper<UserDataController, UserData, UserData_GetInfoById_Output_APIItem>(this);
        }

        #endregion

        #region 錯誤訊息 - 更新密碼

        private static string UpdatePWDbFailed(Exception e)
        {
            return $"更新密碼時失敗：{e.Message}！";
        }

        private const string UpdatePWPasswordNotEncryptable = "密碼只允許英數字！";
        private const string DailyChangePasswordLimitExceeded = "此帳號今日已無法再修改密碼！";
        private const string UpdatePWOriginalPasswordIncorrect = "原密碼不符，請重新確認！";
        private const string UpdatePWNewPasswordShouldBeDifferent = "新密碼不可與原密碼相同！";

        #endregion

        #region Login

        /// <summary>
        ///     驗證使用者登入，無誤則會回傳使用者的 Username 和 JWT Token。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>UserData_Login_Output_APIItem</returns>
        [HttpPost]
        public async Task<string> Login(UserData_Login_Input_APIItem input)
        {
            // 驗證
            UserData queried = await LoginGetUserData(input);

            bool isValidated = LoginValidateCredential(input, queried)
                               && await LoginValidateJwtExpired(queried);

            if (!isValidated)
                return GetResponseJson();

            // 登入都成功後，回傳部分使用者資訊，以及使用者的權限資訊。
            // 先建立 claims
            (string jwt, bool isAdmin) result = LoginCreateJwt(queried);

            DateTime userLastPasswordChangeDate = await GetUserLastPasswordChangeDate(queried);
            int currentMaxPasswordDays = await GetPasswordExpireDays();

            var output = new UserData_Login_Output_APIItem
            {
                UID = queried.UID,
                Username = queried.UserName,
                JwtToken = result.jwt,
                IsAdministrator = result.isAdmin,
                DaysUntilPasswordExpires =
                    (userLastPasswordChangeDate.AddDays(currentMaxPasswordDays).Date - DateTime.Now.Date).Days,
                IpAddress = Request.UserHostAddress
            };

            bool isUpdateSuccessful = await LoginUpdateDb(queried, result.jwt);

            if (!isUpdateSuccessful)
                return GetResponseJson();

            JwtAuthFilter.StartIdleTimer(output.UID);
            return GetResponseJson(output);
        }

        private async Task<bool> LoginValidateJwtExpired(UserData queried)
        {
            // 如果設定檔有打開同一使用者只能登入一次，且沒有打開可強制中斷連線
            // 則檢查目前登記中的 JWT
            // 若 JWT 尚未過期，不允許新的登入

            string enforceOneSessionPerUserCode =
                ((int)StaticCodeSafetyControlCode.EnforceOneSessionPerUser).ToString();
            string newCanKickOldCode = ((int)StaticCodeSafetyControlCode.NewSessionTerminatesOld).ToString();

            Dictionary<string, bool> data = await DC.B_StaticCode
                .Where(bsc => bsc.CodeType == (int)StaticCodeType.SafetyControl)
                .Where(bsc => bsc.Code == enforceOneSessionPerUserCode
                              || bsc.Code == newCanKickOldCode)
                .ToDictionaryAsync(bsc => bsc.Code, bsc => bsc.SortNo == 1);

            bool enforceOneSessionPerUser = data.GetValueOrDefault(enforceOneSessionPerUserCode);

            // 沒限制同時登入，不再檢查
            if (!enforceOneSessionPerUser)
                return true;

            try
            {
                DateTime expireTime = queried.LoginDate.GetNextJwtExpireDateTime();
                bool hasExpired = expireTime < DateTime.Now;

                // 上一組登入過期了，開放登入

                if (hasExpired) return true;

                // 上一組登入還沒過期

                // 特殊情況：同一裝置可以踢掉舊的登入
                // 每個 uid 發行 JWT 時紀錄 IP，如果這組 IP 不符合當前要求的 IP 

                bool newCanKickOld = data.GetValueOrDefault(newCanKickOldCode);

                if (newCanKickOld)
                {
                    // 檢查這個 uid 最後登入時登記的 IP，如果符合現在的 IP 就開放再度登入
                    // 如果還沒有登記，視為通過

                    userIdToLastLoginIpAddress.TryGetValue(queried.UID, out string lastIpAddress);

                    if (lastIpAddress.IsNullOrWhiteSpace()) return true;

                    if (lastIpAddress == Request.UserHostAddress) return true;
                }

                AddError(new BusinessError(1,
                    $"同一使用者，同一時間只能登入一次。請於 {expireTime.ToFormattedStringDateTime()} 之後再嘗試登入。"));

                return false;
            }
            catch (SecurityTokenExpiredException)
            {
                // 過期表示可以登了，給過
                return true;
            }
        }

        private async Task<DateTime> GetUserLastPasswordChangeDate(UserData userData)
        {
            UserPasswordLog lastChange = await DC.UserPasswordLog
                .Where(upl => upl.UID == userData.UID)
                .Where(upl => upl.Type == (int)UserPasswordLogType.ChangePassword)
                .OrderByDescending(upl => upl.CreDate)
                .FirstOrDefaultAsync();

            return lastChange?.CreDate ?? userData.CreDate;
        }

        private async Task<int> GetPasswordExpireDays()
        {
            return await DC.B_StaticCode
                .Where(bsc => bsc.CodeType == (int)StaticCodeType.SafetyControl)
                .Where(bsc => bsc.Code == ((int)StaticCodeSafetyControlCode.PasswordExpireDays).ToString())
                .Select(bsc => bsc.SortNo)
                .FirstOrDefaultAsync();
        }

        private static IDictionary<int, string> userIdToLastLoginIpAddress = new Dictionary<int, string>();

        private (string, bool) LoginCreateJwt(UserData queried)
        {
            (List<Claim> claims, bool isAdmin) claimAndIsAdmin = LoginCreateClaims(queried);

            // 依據客戶需求，調整有效期間為登入當下到當天晚上 23:59:59
            // 避免在白天過期導致操作中斷
            DateTime start = DateTime.Now;
            DateTime end = start.GetNextJwtExpireDateTime();
            int minutes = start.MinutesUntil(end);

            string jwt =
                JwtHelper.GenerateToken(JwtConstants.Secret, minutes, claimAndIsAdmin.claims);

            userIdToLastLoginIpAddress[queried.UID] = Request.UserHostAddress;
            return (jwt, claimAndIsAdmin.isAdmin);
        }

        private async Task<bool> LoginUpdateDb(UserData queried, string jwt)
        {
            // 1. 更新 JWT 欄位
            // 2. 更新 LoginDate
            // 3. 儲存至 DB
            bool isProcessSuccessful = await this.StartValidate()
                .SkipIfAlreadyInvalid()
                .ValidateAsync(_ => UpdateJWT(queried, jwt), (_, e) => AddError(UpdateDbFailed(e)))
                .Validate(_ => UpdateUserLoginDate(queried))
                .ValidateAsync(_ => DC.SaveChangesStandardProcedureAsync(queried.UID, Request),
                    (_, e) => AddError(UpdateDbFailed(e)))
                .IsValid();

            return isProcessSuccessful;
        }

        private static (List<Claim>, bool) LoginCreateClaims(UserData queried)
        {
            var claims = new List<Claim>
            {
                // queried 已經在上面驗證為非 null。目前專案使用 C# 版本不支援 !，所以以此代替。 
                // ReSharper disable once PossibleNullReferenceException
                new Claim(JwtConstants.UidClaimType, queried.UID.ToString()),
                new Claim(ClaimTypes.Role, AuthorizeTypeSingletonFactory.User.GetRoleValue())
            };

            bool isAdmin = false;
            // 特殊規格：如果擁有特殊 GID 的權限，則認識為管理員
            if (queried.M_Group_User.Any(groupUser => groupUser.GroupData.IsAdministrator))
            {
                claims.Add(new Claim(ClaimTypes.Role, AuthorizeTypeSingletonFactory.Admin.GetRoleValue()));
                isAdmin = true;
            }

            return (claims, isAdmin);
        }

        private bool LoginValidateCredential(UserData_Login_Input_APIItem input, UserData queried)
        {
            // 1. 先查詢是否確實有這個帳號
            // 2. 確認帳號的啟用 Flag 與刪除 Flag 
            // 3. 有帳號，才驗證登入密碼
            bool isValidated = queried.StartValidate(true)
                .Validate(q => q != null, () => AddError(NotFound("使用者帳號", nameof(input.LoginAccount))))
                .Validate(q => q.ActiveFlag && !q.DeleteFlag,
                    () => AddError(NotFound("使用者帳號", nameof(input.LoginAccount))))
                .Validate(q => ValidatePassword(input.LoginPassword, q.LoginPassword),
                    () => AddError(1, LoginPasswordIncorrect))
                .IsValid();
            return isValidated;
        }

        private async Task<UserData> LoginGetUserData(UserData_Login_Input_APIItem input)
        {
            var queried = input.LoginAccount.HasContent()
                ? await DC.UserData
                    .Include(u => u.M_Group_User)
                    .Include(u => u.M_Group_User.Select(mgu => mgu.GroupData))
                    .FirstOrDefaultAsync(u => u.LoginAccount == input.LoginAccount)
                : null;
            return queried;
        }

        /// <summary>
        /// 更新使用者的 JWT 資料，並在 UserPasswordLog 寫一筆紀錄。
        /// </summary>
        /// <param name="user">userdata</param>
        /// <param name="jwt">新的 Token</param>
        private async Task UpdateJWT(UserData user, string jwt)
        {
            UserPasswordLog newLog = new UserPasswordLog
            {
                UID = user.UID,
                Type = (int)UserPasswordLogType.Login
            };

            await DC.UserPasswordLog.AddAsync(newLog);

            user.JWT = jwt;
        }

        /// <summary>
        /// 更新使用者的登入日期時間。
        /// </summary>
        /// <param name="user">使用者資料</param>
        private static void UpdateUserLoginDate(UserData user)
        {
            user.LoginDate = DateTime.Now;
        }

        #endregion

        #region Submit

        /// <summary>
        ///     更新使用者資料。<br />
        ///     如果 LoginPassword 有輸入且與資料庫不同時，則會多驗證 OriginalPassword。<br />
        ///     Note 欄位無論有無輸入都會更新，因此呼叫者輸入時需要自行維持 Note 的內容。
        /// </summary>
        /// <param name="input">輸入資料</param>
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag,
            nameof(UserData_Submit_Input_APIItem.UID), null)]
        public async Task<string> Submit(UserData_Submit_Input_APIItem input)
        {
            return await _submitHelper.Submit(input);
        }

        public bool SubmitIsAdd(UserData_Submit_Input_APIItem input)
        {
            // 特殊規格：使用者資料的新增是透過 SignUp 端點，因此這裡永遠回傳 false 以讓 Helper 進入修改模式。
            return false;
        }

        #region Submit - Add

        public Task<bool> SubmitAddValidateInput(UserData_Submit_Input_APIItem input)
        {
            // UserData.Submit 不支援新增，但因為利用 Helper 介面，必須有實作方法。
            // 這個方法永遠不該被呼叫。
            throw new NotSupportedException();
        }

        public Task<UserData> SubmitCreateData(UserData_Submit_Input_APIItem input)
        {
            // UserData.Submit 不支援新增，但因為利用 Helper 介面，必須有實作方法。
            // 這個方法永遠不該被呼叫。
            throw new NotSupportedException();
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(UserData_Submit_Input_APIItem input)
        {
            bool isValid = await input.StartValidate()
                .Validate(i => i.UID.IsAboveZero(), () => AddError(EmptyNotAllowed("使用者 ID", nameof(input.UID))))
                .Validate(i => i.Username.HasContent() && i.Username.Length.IsInBetween(1, 50),
                    () => AddError(LengthOutOfRange("使用者名稱", nameof(input.Username), 1, 50)))
                .Validate(i => i.LoginAccount.HasContent() && i.LoginAccount.Length.IsInBetween(1, 100),
                    () => AddError(LengthOutOfRange("使用者帳號", nameof(input.LoginAccount), 1, 100)))
                .ValidateAsync(
                    async i => !await DC.UserData.AnyAsync(ud =>
                        !ud.DeleteFlag && ud.LoginAccount == i.LoginAccount && ud.UID != input.UID),
                    () => AddError(AlreadyExists("使用者帳號", nameof(input.LoginAccount))))
                .ValidateAsync(async i => await DC.D_Department.ValidateIdExists(i.DDID, nameof(D_Department.DDID)),
                    () => AddError(NotFound("部門 ID", nameof(input.DDID))))
                .ValidateAsync(async i => await ValidateDepartmentCapacity(i.DDID, new[] { input.UID }),
                    () => AddError(2, "指定的部門已達編制人數上限！"))
                .ValidateAsync(
                    async i => await DC.GroupData.ValidateIdExists(i.GID, nameof(GroupData.GID)),
                    () => AddError(NotFound("角色 ID", nameof(input.GID))))
                .Validate(i => i.LoginPassword.IsNullOrWhiteSpace() || i.LoginPassword.Length.IsInBetween(1, 100),
                    () => AddError(OutOfRange("使用者密碼", nameof(input.LoginPassword), 1, 100)))
                .Validate(i => i.LoginPassword.IsNullOrWhiteSpace() || i.LoginPassword.IsEncryptablePassword(),
                    () => AddError(1, PasswordAlphanumericOnly))
                .IsValid();

            return await Task.FromResult(isValid);
        }

        public IQueryable<UserData> SubmitEditQuery(UserData_Submit_Input_APIItem input)
        {
            return DC.UserData.Where(u => u.UID == input.UID);
        }

        public void SubmitEditUpdateDataFields(UserData data, UserData_Submit_Input_APIItem input)
        {
            // 只在欄位有輸入任何資料時，才更新對應欄位
            data.UserName = input.Username.IsNullOrWhiteSpace() ? data.UserName : input.Username;
            data.LoginAccount =
                input.LoginAccount.IsNullOrWhiteSpace() ? data.LoginAccount : input.LoginAccount;

            // Note 是可選欄位，因此呼叫者應該保持原始內容
            data.Note = input.Note;

            data.DDID = input.DDID;

            // 如果是管理員，才允許繼續更新後續的欄位
            if (!FilterStaticTools.HasRoleInRequest(HttpContext.Request, AuthorizeBy.Admin)) return;

            // 如果密碼有變動時，更新密碼。
            string newPassword = EncryptPassword(input.LoginPassword);
            if (!input.LoginPassword.IsNullOrWhiteSpace() && newPassword != data.LoginPassword)
            {
                WriteUserChangePasswordLog(data.UID, data.LoginPassword, newPassword);
                data.LoginPassword = newPassword;
            }

            // 只在是管理員時才允許修改啟用狀態
            data.ActiveFlag = input.ActiveFlag;

            // 資料庫建模是 User 一對多 Group，但現在看到的 Wireframe 似乎規劃為每位使用者僅一個權限組
            // 所以在這裡
            // 1. 在 M_Group_User 中查詢此使用者有幾筆權限，如果有多筆就先清空至唯一一筆。
            // 2. 如果沒有資料就建一筆。
            // 3. 將唯一一筆 M_Group_User 指向 input 指定的 GID。

            var groupUsers = DC.M_Group_User.Where(gu => gu.UID == data.UID).ToList();

            // 無資料時，新增一筆資料
            if (!groupUsers.Any())
            {
                // 新增一筆權限資料並返回
                var groupUser = new M_Group_User
                {
                    GID = input.GID,
                    UID = data.UID
                };

                DC.M_Group_User.Add(groupUser);
                return;
            }

            // 有多筆資料時，只保留一筆
            if (groupUsers.Count > 1)
                DC.M_Group_User.RemoveRange(groupUsers.Skip(1));

            // 更新權限資料
            var newGroupUser = groupUsers.First();
            newGroupUser.GID = input.GID;
        }

        private void WriteUserChangePasswordLog(int uid, string oldPassword, string newPassword)
        {
            // 先檢查同一天是不是已經有超過上限次數的修改密碼歷史了
            int dailyLimit = GetPasswordEditTimeDaily();

            // 驗證
            // |- a. 修改密碼次數
            // +- b. 此密碼是否還不能重用

            UserPasswordLog[] logs = DC.UserPasswordLog
                .Where(log => log.UID == uid && log.Type == (int)UserPasswordLogType.ChangePassword)
                .ToArray();

            int todayChangedTimes = logs.Count(log => log.CreDate.Date == DateTime.Now.Date);

            if (todayChangedTimes > dailyLimit)
                AddError(5, DailyChangePasswordLimitExceeded);

            int uniquePasswordCountLimit = GetUniquePasswordCountLimit();

            UserPasswordLog[] updatePasswordHistories = logs
                .OrderByDescending(log => log.CreDate)
                .Take(uniquePasswordCountLimit)
                .ToArray();

            // 新密碼：所有 n 筆都不能出現
            // 舊密碼：當歷史紀錄小於 n 筆時，需要多確認第 1 筆的舊資料
            // （第 i 筆的新資料為第 i+1 筆的舊資料，所以只有第 1 筆的舊資料需要確認）
            if (updatePasswordHistories.Any(log => log.NewPassword == newPassword)
                || updatePasswordHistories.Length < uniquePasswordCountLimit
                && updatePasswordHistories.LastOrDefault()?.OldPassword == newPassword)
                AddError(4, $"不可重覆使用前 {uniquePasswordCountLimit} 組密碼！");

            // 沒有才繼續下去
            if (HasError())
                return;

            UserPasswordLog newLog = new UserPasswordLog
            {
                UID = uid,
                Type = (int)UserPasswordLogType.ChangePassword,
                OldPassword = oldPassword,
                NewPassword = newPassword
            };

            DC.UserPasswordLog.Add(newLog);
        }

        private int GetUniquePasswordCountLimit()
        {
            return DC.B_StaticCode.Where(sc =>
                    sc.ActiveFlag && !sc.DeleteFlag && sc.CodeType == (int)StaticCodeType.SafetyControl)
                .Where(sc => sc.Code == ((int)StaticCodeSafetyControlCode.PasswordNoReuseCount).ToString())
                .Select(sc => sc.SortNo)
                .FirstOrDefault();
        }

        #endregion

        #endregion

        #region 密碼驗證

        /// <summary>
        ///     針對使用者密碼進行加密。<br />
        /// </summary>
        /// <param name="password">使用者密碼</param>
        /// <returns>result: 加密結果<br />encryptedPassword: 加密後的字串</returns>
        /// <exception cref="ValidationException">使用者密碼包含半形英數字以外的字元時，回傳此錯誤</exception>
        private static string EncryptPassword(string password)
        {
            // 目前使用的加密方法只允許英數字
            // sanitize
            ThrowIfPasswordIsNotEncryptable(password);

            return HSM.Enc_1(password);
        }

        private static void ThrowIfPasswordIsNotEncryptable(string password)
        {
            if (!password.IsEncryptablePassword())
                throw new ValidationException(PasswordAlphanumericOnly);
        }

        /// <summary>
        ///     驗證輸入密碼是否與指定的加密字串相符。
        /// </summary>
        /// <param name="input">原始輸入的密碼</param>
        /// <param name="data">資料庫的已加密密碼</param>
        /// <returns>
        ///     true：相符<br />
        ///     false：不相符，或密碼輸入包含半形英數字以外的字元
        /// </returns>
        private static bool ValidatePassword(string input, string data)
        {
            if (!input.IsEncryptablePassword())
                return false;

            try
            {
                return EncryptPassword(input) == data;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region DeleteItem

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.DeleteFlag)]
        public async Task<string> DeleteItem(DeleteItem_Input_APIItem input)
        {
            // 若 deleteFlag 為 false 時，驗證是否有其他不同 UID、相同 LoginAccount、非刪除狀態的資料，如果有，打回。

            foreach (var item in input.Items)
            {
                if (!(item.DeleteFlag is false)) continue;

                // 先取得 UserData 以便取得 LoginAccount
                UserData target = await DC.UserData.FirstOrDefaultAsync(ud => ud.UID == (item.Id ?? 0));
                if (target is null)
                {
                    AddError(NotFound("使用者 ID", nameof(UserData.UID)));
                    return GetResponseJson();
                }

                bool hasOtherAccount = await DC.UserData.AnyAsync(ud =>
                    ud.UID != (item.Id ?? 0) && !ud.DeleteFlag && ud.LoginAccount == target.LoginAccount);

                // 沒有其他帳號，通過
                if (!hasOtherAccount) continue;

                AddError(AlreadyExists("欲復活的使用者帳號", nameof(UserData.LoginAccount)));
            }

            // 驗證要復活的帳號所屬部門沒滿
            HashSet<int> toReviveIds = input.GetUniqueReviveId();
            UserData[] userToRevive = await DC.UserData.Where(ud => toReviveIds.Contains(ud.UID)).ToArrayAsync();

            foreach (IGrouping<int, UserData> group in userToRevive.GroupBy(u => u.DDID))
            {
                bool validation = await ValidateDepartmentCapacity(group.Key, group.Select(ud => ud.UID));

                if (validation) continue;

                string users = String.Join(",", group.Select(ud => ud.UID));
                AddError(1, $"欲復活的使用者 {users} 所屬部門現已達編制人數上限，請調整編制人數，或是先變更現有使用者的部門！");
            }


            if (HasError())
                return GetResponseJson();

            return await _deleteItemHelper.DeleteItem(input);
        }

        public IQueryable<UserData> DeleteItemsQuery(IEnumerable<int> ids)
        {
            return DC.UserData.Where(u => ids.Contains(u.UID));
        }

        #endregion

        #region ChangeActive

        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
            int departmentId = await DC.UserData.Where(ud => ud.UID == id).Select(ud => ud.DDID).FirstOrDefaultAsync();
            if (activeFlag == true && !await ValidateDepartmentCapacity(departmentId, new[] { id }))
            {
                AddError(1, "欲啟用的帳號所屬部門現在已達編制人數上限！");
                return GetResponseJson();
            }

            return await _changeActiveHelper.ChangeActive(id, activeFlag);
        }

        public IQueryable<UserData> ChangeActiveQuery(int id)
        {
            return DC.UserData.Where(ud => ud.UID == id);
        }

        #endregion

        #region UpdatePW

        /// <summary>
        /// 更新使用者密碼。
        /// </summary>
        /// <param name="input">
        /// <see cref="UserData_UpdatePW_Input_APIItem" />
        /// </param>
        /// <returns>通用回傳訊息格式</returns>
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.None, true)]
        public async Task<string> UpdatePW(UserData_UpdatePW_Input_APIItem input)
        {
            // 1. 驗證。
            // |- a. 驗證所有輸入均有值
            // |- b. 驗證密碼最小長度
            // |- c. 驗證新密碼可加密
            // +- d. 成功更新資料庫
            int passwordMinLength = GetPasswordMinLength();
            bool isValid = input
                .StartValidate()
                .SkipIfAlreadyInvalid()
                .Validate(i => i.OriginalPassword != i.NewPassword,
                    () => AddError(1, UpdatePWNewPasswordShouldBeDifferent))
                .Validate(i => i.OriginalPassword.HasContent(),
                    () => AddError(EmptyNotAllowed("原始密碼", nameof(input.OriginalPassword))))
                .Validate(i => i.OriginalPassword.Length.IsInBetween(1, 100),
                    () => AddError(LengthOutOfRange("原始密碼", nameof(input.OriginalPassword), 1, 100)))
                // 如果無法加密，表示密碼有意外字元，同時 API 也應該在寫入欄位前就擋掉
                // 所以這種情況視為密碼錯誤，直接返回。
                .Validate(i => i.OriginalPassword.IsEncryptablePassword(),
                    () => AddError(2, UpdatePWOriginalPasswordIncorrect))
                .Validate(i => i.NewPassword.HasContent(),
                    () => AddError(EmptyNotAllowed("新密碼", nameof(input.NewPassword))))
                .Validate(i => i.NewPassword.Length.IsInBetween(passwordMinLength, 100),
                    () => AddError(LengthOutOfRange("新密碼", nameof(input.NewPassword), passwordMinLength, 100)))
                .Validate(i => i.NewPassword.IsEncryptablePassword(), () => AddError(3, UpdatePWPasswordNotEncryptable))
                .IsValid();

            if (!isValid)
                return GetResponseJson();

            // 2. 查詢資料
            int uid = GetUid();
            UserData userData =
                await DC.UserData.FirstOrDefaultAsync(ud => ud.ActiveFlag && !ud.DeleteFlag && ud.UID == uid);

            if (userData is null)
            {
                AddError(NotFound("您目前的使用者 ID", nameof(UserData.UID)));
                return GetResponseJson();
            }

            // 3. 對照舊密碼

            bool oldPasswordIsCorrect = ValidatePassword(input.OriginalPassword, userData.LoginPassword);

            if (!oldPasswordIsCorrect)
            {
                AddError(2, UpdatePWOriginalPasswordIncorrect);
                return GetResponseJson();
            }

            // 4. 更新 DB
            await input
                .StartValidate(true)
                .ValidateAsync(async i => await UpdatePasswordForUserData(userData, i.NewPassword));

            // 5. 回傳通用訊息格式。
            return GetResponseJson();
        }

        private int GetPasswordEditTimeDaily()
        {
            return DC.B_StaticCode.Where(sc =>
                    sc.ActiveFlag && !sc.DeleteFlag && sc.CodeType == (int)StaticCodeType.SafetyControl)
                .Where(sc => sc.Code == ((int)StaticCodeSafetyControlCode.PasswordChangeDailyLimit).ToString())
                .Select(sc => sc.SortNo)
                .FirstOrDefault();
        }

        private int GetPasswordMinLength()
        {
            return DC.B_StaticCode.Where(sc =>
                    sc.CodeType == (int)StaticCodeType.SafetyControl && sc.ActiveFlag && !sc.DeleteFlag)
                .Where(sc => sc.Code == ((int)StaticCodeSafetyControlCode.PasswordMinLength).ToString())
                .Select(sc => sc.SortNo)
                .FirstOrDefault();
        }

        private async Task UpdatePasswordForUserData(UserData userData, string inputPassword)
        {
            // 1. 密碼沒有變時，不做任何事
            string newPassword = EncryptPassword(inputPassword);
            if (newPassword == userData.LoginPassword)
                return;

            // 2. 更新資料，更新失敗時拋錯
            try
            {
                WriteUserChangePasswordLog(userData.UID, userData.LoginPassword, newPassword);
                if (HasError())
                    return;
                userData.LoginPassword = newPassword;
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
            }
            catch (Exception e)
            {
                throw new DbUpdateException(UpdatePWDbFailed(e));
            }
        }

        #endregion

        #region GetList

        /// <summary>
        ///     取得使用者列表。
        /// </summary>
        /// <param name="input">輸入資料。請參照 <see cref="UserData_GetList_Input_APIItem" />。</param>
        /// <returns>回傳結果。請參照 <see cref="UserData_GetList_Output_APIItem" />，以及 <see cref="UserData_GetList_Output_Row_APIItem" />。</returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag)]
        public async Task<string> GetList(UserData_GetList_Input_APIItem input)
        {
            return await _getListPagedHelper.GetPagedList(input);
        }

        public async Task<bool> GetListPagedValidateInput(UserData_GetList_Input_APIItem input)
        {
            // 此輸入不需驗證
            return await Task.FromResult(true);
        }

        public IOrderedQueryable<UserData> GetListPagedOrderedQuery(UserData_GetList_Input_APIItem input)
        {
            var query = DC.UserData
                .Include(u => u.D_Department)
                .Include(u => u.D_Department.M_Department_Category)
                .Include(u => u.D_Department.D_Company)
                .Include(u => u.M_Group_User)
                .Include(u => u.M_Group_User.Select(mgu => mgu.GroupData))
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(u => u.UserName.Contains(input.Keyword));

            if (input.DCID.IsAboveZero())
                query = query.Where(u => u.D_Department.DCID == input.DCID);

            if (input.DDID.IsAboveZero())
                query = query.Where(u => u.DDID == input.DDID);

            if (input.GID.IsAboveZero())
                query = query.Where(u => u.M_Group_User.Any(mgu => mgu.GID == input.GID));

            return query.OrderBy(u => u.UID);
        }

        public async Task<UserData_GetList_Output_Row_APIItem> GetListPagedEntityToRow(UserData entity)
        {
            return await Task.FromResult(new UserData_GetList_Output_Row_APIItem
            {
                Uid = entity.UID,
                Username = entity.UserName,
                Company = entity.D_Department?.D_Company?.TitleC ?? entity.D_Department?.D_Company?.TitleE ?? "",
                Department = entity.D_Department?.TitleC ?? entity.D_Department?.TitleE ?? "",
                // 目前系統每個使用者只會有一個 Group
                Role = entity.M_Group_User
                    .OrderBy(mgu => mgu.MID)
                    .FirstOrDefault(mgu => mgu.GroupData != null
                                           && !mgu.GroupData.DeleteFlag
                                           && mgu.GroupData.ActiveFlag)
                    ?.GroupData.Title ?? IoConstants.DefaultRole
            });
        }

        #endregion

        #region GetInfoById

        /// <summary>
        ///     查詢單筆使用者資料。
        /// </summary>
        /// <param name="id">UID</param>
        /// <returns>
        ///     <see cref="UserData_GetInfoById_Output_APIItem" />
        /// </returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.ShowFlag, "ID", null)]
        public async Task<string> GetInfoById(int id)
        {
            return await _getInfoByIdHelper.GetInfoById(id);
        }

        public IQueryable<UserData> GetInfoByIdQuery(int id)
        {
            return DC.UserData
                .Include(ud => ud.M_Group_User)
                .Include(ud => ud.M_Group_User.Select(mgu => mgu.GroupData))
                .Where(ud => ud.UID == id);
        }

        public async Task<UserData_GetInfoById_Output_APIItem> GetInfoByIdConvertEntityToResponse(UserData entity)
        {
            return await Task.FromResult(new UserData_GetInfoById_Output_APIItem
            {
                UID = entity.UID,
                Username = entity.UserName,
                LoginAccount = entity.LoginAccount,
                LoginPassword = HSM.Des_1(entity.LoginPassword),
                DDID = entity.DDID,
                GID = entity.M_Group_User
                    .Where(mgu => mgu.GroupData.ActiveFlag && !mgu.GroupData.DeleteFlag)
                    .OrderBy(mgu => mgu.MID)
                    .Select(mgu => mgu.GID)
                    .FirstOrDefault(),
                Note = entity.Note ?? ""
            });
        }

        #endregion

        #region BatchSubmitDepartment

        /// <summary>
        /// 更新一筆或多筆使用者的部門設定。
        /// </summary>
        /// <param name="input">輸入資料，參照 <see cref="UserData_BatchSubmitDepartment_Input_APIItem"/>。</param>
        /// <returns>通用訊息回傳格式</returns>
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> BatchSubmitDepartment(UserData_BatchSubmitDepartment_Input_APIItem input)
        {
            // 0. enumerate
            UserData_BatchSubmitDepartment_Input_Row_APIItem[] itemsArray = input.Items?.ToArray();

            // 1. 驗證輸入
            bool isCollectionValid = BatchSubmitDepartmentValidateCollection(itemsArray);

            if (!isCollectionValid)
                return GetResponseJson();

            // 先查出所有符合輸入 UID 的 UserData，可以避免驗證和修改時重複查兩次 UserData
            // 這裡 IDE 會認為 itemsArray 可能為 null，但在 Collection 驗證時已經排除掉此可能性，所以這裡停用警告
            // ReSharper disable once AssignNullToNotNullAttribute
            IEnumerable<int> inputUserIds = itemsArray.Select(i => i.UID);
            Dictionary<int, UserData> data = await BatchSubmitDepartmentGetUserDictionary(inputUserIds);

            bool isElementValid = await BatchSubmitDepartmentValidateElements(input, data);

            if (!isElementValid)
                return GetResponseJson();

            // 驗證要變動到的帳號所屬部門沒滿
            foreach (IGrouping<int, UserData_BatchSubmitDepartment_Input_Row_APIItem> group in itemsArray.GroupBy(i =>
                         i.DDID))
            {
                bool validation = await ValidateDepartmentCapacity(group.Key, group.Select(ud => ud.UID));

                if (validation) continue;

                string users = String.Join(",", group.Select(ud => ud.UID));
                AddError(1, $"欲復活的使用者 {users} 所屬部門現已達編制人數上限，請調整編制人數，或是先變更現有使用者的部門！");
            }

            if (HasError())
                return GetResponseJson();

            // 2. 更新資料
            BatchSubmitDepartmentUpdate(itemsArray, data);

            // 3. 寫入 DB
            await BatchSubmitDepartmentWriteToDb();

            return GetResponseJson();
        }

        private async Task BatchSubmitDepartmentWriteToDb()
        {
            try
            {
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
            }
            catch (Exception e)
            {
                AddError(UpdateDbFailed(e));
            }
        }

        private static void BatchSubmitDepartmentUpdate(UserData_BatchSubmitDepartment_Input_Row_APIItem[] itemsArray,
            Dictionary<int, UserData> data)
        {
            foreach (UserData_BatchSubmitDepartment_Input_Row_APIItem item in itemsArray)
            {
                data[item.UID].DDID = item.DDID;
            }
        }

        private async Task<bool> BatchSubmitDepartmentValidateElements(
            UserData_BatchSubmitDepartment_Input_APIItem input, Dictionary<int, UserData> data)
        {
            return await input.Items.StartValidateElements()
                .Validate(i => i.UID.IsAboveZero(),
                    i => AddError(WrongFormat($"對象使用者 ID（{i.UID}）", nameof(i.UID))))
                .Validate(i => i.DDID.IsAboveZero(),
                    i => AddError(EmptyNotAllowed($"部門 ID（UID：{i.UID}）", nameof(i.DDID))))
                .SkipIfAlreadyInvalid()
                .Validate(i => data.ContainsKey(i.UID),
                    i => AddError(NotFound($"對象使用者 ID {i.UID}", nameof(i.UID))))
                .ValidateAsync(async i => await DC.D_Department.ValidateIdExists(i.DDID, nameof(D_Department.DDID)),
                    i => AddError(NotFound($"部門 ID {i.DDID}（UID：{i.UID}）", nameof(i.DDID))))
                .IsValid();
        }

        private async Task<Dictionary<int, UserData>> BatchSubmitDepartmentGetUserDictionary(
            IEnumerable<int> inputUserIds)
        {
            return await DC.UserData
                .Where(ud => ud.ActiveFlag)
                .Where(ud => !ud.DeleteFlag)
                .Where(ud => inputUserIds.Contains(ud.UID))
                .ToDictionaryAsync(ud => ud.UID, ud => ud);
        }

        private bool BatchSubmitDepartmentValidateCollection(
            UserData_BatchSubmitDepartment_Input_Row_APIItem[] itemsArray)
        {
            return itemsArray.StartValidate()
                .Validate(items => items != null && items.Any(),
                    () => AddError(
                        EmptyNotAllowed("欲更新的資料", nameof(UserData_BatchSubmitDepartment_Input_APIItem.Items))))
                .Validate(items => items.GroupBy(i => i.UID).Count() == items.Count(),
                    () => AddError(CopyNotAllowed("對象使用者 ID",
                        nameof(UserData_BatchSubmitDepartment_Input_Row_APIItem.UID))))
                .IsValid();
        }

        #endregion

        #region BatchSubmitGroup

        /// <summary>
        /// 更新一筆或多筆使用者的角色設定。
        /// </summary>
        /// <param name="input">輸入資料，參照 <see cref="UserData_BatchSubmitDepartment_Input_APIItem"/>。</param>
        /// <returns>通用訊息回傳格式</returns>
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag)]
        public async Task<string> BatchSubmitGroup(UserData_BatchSubmitGroup_Input_APIItem input)
        {
            /* TODO:
             目前批量修改僅 UserData 發現兩個需求，所以先以特例方式寫，可以發現結構長得非常類似。
             如果未來發現更多類似需求，或是這兩個頻繁需要同步修改時，須考慮 Helper 化。
            */

            // 0. enumerate
            UserData_BatchSubmitGroup_Input_Row_APIItem[] itemsArray = input.Items?.ToArray();

            // 1. 驗證輸入
            bool isCollectionValid = BatchSubmitGroupValidateCollection(itemsArray);

            if (!isCollectionValid)
                return GetResponseJson();

            // 先查出所有符合輸入 UID 的 UserData，可以避免驗證和修改時重複查兩次 UserData
            // 這裡 IDE 會認為 itemsArray 可能為 null，但在 Collection 驗證時已經排除掉此可能性，所以這裡停用警告
            // ReSharper disable once AssignNullToNotNullAttribute
            IEnumerable<int> inputUserIds = itemsArray.Select(i => i.UID);
            Dictionary<int, UserData> data = await BatchSubmitGroupGetUserDictionary(inputUserIds);

            bool isElementValid = await BatchSubmitGroupValidateElements(input, data);

            if (!isElementValid)
                return GetResponseJson();

            // 2. 更新資料
            BatchSubmitGroupUpdate(itemsArray, data);

            // 3. 寫入 DB
            await BatchSubmitGroupWriteToDb();

            return GetResponseJson();
        }

        private async Task BatchSubmitGroupWriteToDb()
        {
            try
            {
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
            }
            catch (Exception e)
            {
                AddError(UpdateDbFailed(e));
            }
        }

        private void BatchSubmitGroupUpdate(UserData_BatchSubmitGroup_Input_Row_APIItem[] itemsArray,
            IReadOnlyDictionary<int, UserData> data)
        {
            foreach (UserData_BatchSubmitGroup_Input_Row_APIItem item in itemsArray)
            {
                UserData user = data[item.UID];
                // 清空既有的 groupUser, 因為目前每個 user 實際上只需要支援一個 groupUser
                DC.M_Group_User.RemoveRange(user.M_Group_User);

                // 寫入新的角色
                user.M_Group_User.Add(new M_Group_User
                {
                    GID = item.GID,
                    UID = user.UID
                });
            }
        }

        private async Task<bool> BatchSubmitGroupValidateElements(UserData_BatchSubmitGroup_Input_APIItem input,
            Dictionary<int, UserData> data)
        {
            return await input.Items.StartValidateElements()
                .Validate(i => i.UID.IsAboveZero(),
                    i => AddError(WrongFormat($"對象使用者 ID（{i.UID}）", nameof(i.UID))))
                .Validate(i => i.GID.IsAboveZero(),
                    i => AddError(EmptyNotAllowed($"角色 ID（UID：{i.UID}）", nameof(i.GID))))
                .SkipIfAlreadyInvalid()
                .Validate(i => data.ContainsKey(i.UID),
                    i => AddError(NotFound($"對象使用者 ID {i.UID}", nameof(i.UID))))
                .ValidateAsync(async i => await DC.GroupData.ValidateIdExists(i.GID, nameof(GroupData.GID)),
                    i => AddError(NotFound($"角色 ID {i.GID}（UID：{i.UID}）", nameof(i.GID))))
                .IsValid();
        }

        private async Task<Dictionary<int, UserData>> BatchSubmitGroupGetUserDictionary(IEnumerable<int> inputUserIds)
        {
            return await DC.UserData
                .Include(ud => ud.M_Group_User)
                .Where(ud => ud.ActiveFlag)
                .Where(ud => !ud.DeleteFlag)
                .Where(ud => inputUserIds.Contains(ud.UID))
                .ToDictionaryAsync(ud => ud.UID, ud => ud);
        }

        private bool BatchSubmitGroupValidateCollection(UserData_BatchSubmitGroup_Input_Row_APIItem[] itemsArray)
        {
            return itemsArray.StartValidate()
                .Validate(items => items != null && items.Any(),
                    () => AddError(EmptyNotAllowed("欲更新的資料", nameof(UserData_BatchSubmitGroup_Input_APIItem.Items))))
                .Validate(items => items.GroupBy(i => i.UID).Count() == items.Count(),
                    () => AddError(CopyNotAllowed("對象使用者 ID",
                        nameof(UserData_BatchSubmitDepartment_Input_Row_APIItem.UID))))
                .IsValid();
        }

        #endregion

        #region Logout

        /// <summary>
        /// 執行使用者登出所需之後端處理。
        /// </summary>
        /// <returns>通用訊息回傳格式</returns>
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.None)]
        public async Task<string> Logout()
        {
            UserData user = await LogoutGetUserData();

            if (user == null)
            {
                AddError(NotFound($"目前登入的使用者（ID {GetUid()}）", nameof(UserData.UID)));
                return GetResponseJson();
            }

            await LogoutUpdateUserDataAndSaveToDb(user);

            if (!HasError())
                JwtAuthFilter.ResetUidIdle(user.UID);

            return GetResponseJson();
        }

        private async Task LogoutUpdateUserDataAndSaveToDb(UserData user)
        {
            try
            {
                user.JWT = String.Empty;
                await LogoutWriteUserLogoutLog();
                await DC.SaveChangesStandardProcedureAsync(GetUid(), Request);
            }
            catch (Exception e)
            {
                AddError(UpdateDbFailed(e));
            }
        }

        private async Task<UserData> LogoutGetUserData()
        {
            int uid = GetUid();
            UserData user =
                await DC.UserData.FirstOrDefaultAsync(ud => ud.UID == uid && !ud.DeleteFlag && ud.ActiveFlag);
            return user;
        }

        private async Task LogoutWriteUserLogoutLog()
        {
            UserPasswordLog logoutLog = new UserPasswordLog
            {
                UID = GetUid(),
                Type = (int)UserPasswordLogType.Logout
            };
            await DC.AddAsync(logoutLog);
        }

        #endregion
    }
}