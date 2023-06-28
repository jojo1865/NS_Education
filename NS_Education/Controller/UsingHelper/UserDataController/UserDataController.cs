using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using BeingValidated;
using NS_Education.Models.APIItems.Common.DeleteItem;
using NS_Education.Models.APIItems.Controller.UserData.UserData.BatchSubmitDepartment;
using NS_Education.Models.APIItems.Controller.UserData.UserData.BatchSubmitGroup;
using NS_Education.Models.APIItems.Controller.UserData.UserData.GetInfoById;
using NS_Education.Models.APIItems.Controller.UserData.UserData.GetList;
using NS_Education.Models.APIItems.Controller.UserData.UserData.Login;
using NS_Education.Models.APIItems.Controller.UserData.UserData.Submit;
using NS_Education.Models.APIItems.Controller.UserData.UserData.UpdatePW;
using NS_Education.Models.Entities;
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

        #region SignUp

        /// <summary>
        ///     註冊使用者資料。過程中會驗證使用者輸入，並在回傳時一併報錯。<br />
        ///     如果過程驗證都通過，才寫入資料庫。
        /// </summary>
        /// <param name="input">輸入資料</param>
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.AddFlag)]
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

            bool isValidated = LoginValidateCredential(input, queried);

            if (!isValidated)
                return GetResponseJson();

            // 登入都成功後，回傳部分使用者資訊，以及使用者的權限資訊。
            // 先建立 claims
            string jwt = LoginCreateJwt(queried);

            var output = new UserData_Login_Output_APIItem
            {
                UID = queried.UID,
                Username = queried.UserName,
                JwtToken = jwt
            };

            bool isUpdateSuccessful = await LoginUpdateDb(queried, jwt);

            return isUpdateSuccessful ? GetResponseJson(output) : GetResponseJson();
        }

        private static string LoginCreateJwt(UserData queried)
        {
            List<Claim> claims = LoginCreateClaims(queried);

            string jwt = JwtHelper.GenerateToken(JwtConstants.Secret, JwtConstants.ExpireMinutes, claims);
            return jwt;
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

        private static List<Claim> LoginCreateClaims(UserData queried)
        {
            var claims = new List<Claim>
            {
                // queried 已經在上面驗證為非 null。目前專案使用 C# 版本不支援 !，所以以此代替。 
                // ReSharper disable once PossibleNullReferenceException
                new Claim(JwtConstants.UidClaimType, queried.UID.ToString()),
                new Claim(ClaimTypes.Role, AuthorizeTypeSingletonFactory.User.GetRoleValue())
            };

            // 特殊規格：如果擁有特殊 GID 的權限，則認識為管理員
            if (queried.M_Group_User.Any(groupUser => groupUser.GID == JwtConstants.AdminGid))
                claims.Add(new Claim(ClaimTypes.Role, AuthorizeTypeSingletonFactory.Admin.GetRoleValue()));

            return claims;
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
        [JwtAuthFilter(AuthorizeBy.Admin | AuthorizeBy.UserSelf, RequirePrivilege.EditFlag,
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
                throw new Exception(DailyChangePasswordLimitExceeded);

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
                throw new Exception($"不可重覆使用前 {uniquePasswordCountLimit} 組密碼！");

            // 沒有才繼續下去
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
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.DeleteFlag)]
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
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(int id, bool? activeFlag)
        {
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
        [JwtAuthFilter(AuthorizeBy.Any, RequirePrivilege.EditFlag, true)]
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
                .ValidateAsync(async i => await UpdatePasswordForUserData(userData, i.NewPassword),
                    (_, e) => AddError(UpdateDbFailed(e)));

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
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.ShowFlag)]
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
        [JwtAuthFilter(AuthorizeBy.Admin | AuthorizeBy.UserSelf, RequirePrivilege.ShowFlag, "ID", null)]
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