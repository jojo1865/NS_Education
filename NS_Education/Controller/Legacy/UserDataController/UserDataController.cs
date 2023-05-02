using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;
using NS_Education.Models.APIItems.UserData.UserData.ChangeActive;
using NS_Education.Models.APIItems.UserData.UserData.GetInfoById;
using NS_Education.Models.APIItems.UserData.UserData.GetList;
using NS_Education.Models.APIItems.UserData.UserData.Login;
using NS_Education.Models.APIItems.UserData.UserData.Submit;
using NS_Education.Models.APIItems.UserData.UserData.UpdatePW;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
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

namespace NS_Education.Controller.Legacy.UserDataController
{
    public class UserDataController : PublicClass,
        IGetListPaged<UserData, UserData_GetList_Input_APIItem, UserData_GetList_Output_Row_APIItem>,
        IDeleteItem<UserData>,
        ISubmit<UserData, UserData_Submit_Input_APIItem>
    {
        #region 初始化

        private readonly IGetListPagedHelper<UserData_GetList_Input_APIItem> _getListPagedHelper;
        private readonly IDeleteItemHelper _deleteItemHelper;
        private readonly ISubmitHelper<UserData_Submit_Input_APIItem> _submitHelper;

        public UserDataController()
        {
            _getListPagedHelper = new
                GetListPagedHelper<UserDataController, UserData, UserData_GetList_Input_APIItem,
                    UserData_GetList_Output_Row_APIItem>(this);

            _deleteItemHelper = new
                DeleteItemHelper<UserDataController, UserData>(this);

            _submitHelper = new
                SubmitHelper<UserDataController, UserData, UserData_Submit_Input_APIItem>(this);
        }

        #endregion

        #region 錯誤訊息 - 通用

        private const string UpdateUidIncorrect = "未提供欲修改的 UID 或格式不正確！";
        private const string UserDataNotFound = "這筆使用者不存在或已被刪除！";

        private static string QueryFailed(Exception e) => $"查詢 DB 時出錯，請確認伺服器狀態：{e.Message}！";

        #endregion

        #region 錯誤訊息 - 註冊/更新

        private const string PasswordAlphanumericOnly = "使用者密碼只允許半形英文字母、數字！";
        private const string SignUpGidIncorrect = "缺少身分 ID 或查無身分資料，無法寫入！";
        private const string SignUpDdidIncorrect = "缺少部門 ID 或查無部門資料，無法寫入！";

        #endregion

        #region 錯誤訊息 - 登入

        private const string LoginAccountNotFound = "查無此使用者帳號，請重新確認！";
        private const string LoginPasswordIncorrect = "使用者密碼錯誤！";

        private static string LoginDateUpdateFailed(Exception e)
            => $"上次登入時間更新失敗，錯誤訊息：{e.Message}！";

        #endregion

        #region 錯誤訊息 - 啟用/停用

        private static string ChangeActiveUpdateFailed(Exception e)
        {
            return $"更新 DB 時出錯，請確認伺服器狀態：{e.Message}";
        }

        #endregion

        #region 錯誤訊息 - 更新密碼

        private static string UpdatePWDbFailed(Exception e) => $"更新密碼時失敗，請確認伺服器狀態：{e.Message}！";
        private const string UpdatePWPasswordNotEncryptable = "密碼只允許英數字！";

        #endregion

        #region 錯誤訊息 - 查詢

        private const string GetUidIncorrect = "缺少 UID，無法寫入！";
        private const string GetUserNotFound = "查無此使用者帳號，請重新確認！";

        #endregion

        #region SignUp

        /// <summary>
        /// 註冊使用者資料。過程中會驗證使用者輸入，並在回傳時一併報錯。<br/>
        /// 如果過程驗證都通過，才寫入資料庫。
        /// </summary>
        /// <param name="input">輸入資料</param>
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.AddFlag)]
        public async Task<string> SignUp(UserData_Submit_Input_APIItem input)
        {
            InitializeResponse();

            // TODO: 引用靜態參數檔，完整驗證使用者密碼

            // sanitize
            if (!input.GID.IsAboveZero() || !DC.GroupData.Any(g => g.GID == input.GID))
                AddError(SignUpGidIncorrect);
            if (!input.DDID.IsAboveZero() || !DC.D_Department.Any(d => d.DDID == input.DDID))
                AddError(SignUpDdidIncorrect);
            if (input.LoginPassword.IsNullOrWhiteSpace())
                AddError(EmptyNotAllowed("使用者密碼"));
            else
            {
                // check and encrypt pw
                try
                {
                    input.LoginPassword = EncryptPassword(input.LoginPassword);
                }
                catch (ValidationException)
                {
                    AddError(PasswordAlphanumericOnly);
                    // 這裡不做提早返回，方便一次顯示更多錯誤訊息給使用者
                }
            }

            // create UserData object, validate the columns along
            // TODO: 引用靜態參數檔，完整驗證使用者欄位
            int requestUid = GetUid();
            UserData newUser = new UserData
            {
                UserName = input.Username.ExecuteIfNullOrWhiteSpace(() => AddError(EmptyNotAllowed("使用者名稱"))),
                LoginAccount = input.LoginAccount.ExecuteIfNullOrWhiteSpace(() => AddError(EmptyNotAllowed("使用者帳號"))),
                LoginPassword = input.LoginPassword,
                Note = input.Note,
                LoginDate = DateTime.Now,
                DDID = input.DDID,
                M_Group_User = new List<M_Group_User>
                {
                    new M_Group_User
                    {
                        GID = input.GID
                    }
                }
            };

            // doesn't write to db if any error raised
            // For postman testing: 若備註欄為特殊值時，不真正寫入資料。
            if (HasError()) return GetResponseJson();

            await DC.UserData.AddAsync(newUser);
            await DC.SaveChangesStandardProcedureAsync(GetUid());

            return GetResponseJson();
        }

        #endregion

        #region Login

        /// <summary>
        /// 驗證使用者登入，無誤則會回傳使用者的 Username 和 JWT Token。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>UserData_Login_Output_APIItem</returns>
        [HttpPost]
        public async Task<string> Login(UserData_Login_Input_APIItem input)
        {
            InitializeResponse();

            // 驗證
            UserData queried = !input.LoginAccount.IsNullOrWhiteSpace()
                ? await DC.UserData
                    .Include(u => u.M_Group_User)
                    .FirstOrDefaultAsync(u => u.LoginAccount == input.LoginAccount)
                : null;

            // 1. 先查詢是否確實有這個帳號
            // 2. 確認帳號的啟用 Flag 與刪除 Flag 
            // 3. 有帳號，才驗證登入密碼
            // 4. 更新使用者的上次登入時間，需更新成功才算登入成功
            bool isValidated = await queried.StartValidate(true)
                .Validate(q => q != null, () => AddError(LoginAccountNotFound))
                .Validate(q => q.ActiveFlag && !q.DeleteFlag, () => AddError(LoginAccountNotFound))
                .Validate(q => ValidatePassword(input.LoginPassword, q.LoginPassword),
                    () => AddError(LoginPasswordIncorrect))
                .ValidateAsync(UpdateUserLoginDate)
                .IsValid();

            if (!isValidated)
                return GetResponseJson();

            // 登入都成功後，回傳部分使用者資訊，以及使用者的權限資訊。
            // 先建立 claims
            var claims = new List<Claim>
            {
                // queried 已經在上面驗證為非 null。目前專案使用 C# 版本不支援 !，所以以此代替。 
                // ReSharper disable once PossibleNullReferenceException
                new Claim(JwtConstants.UidClaimType, queried.UID.ToString()),
                new Claim(ClaimTypes.Role, AuthorizeTypeSingletonFactory.User.GetRoleValue()),
            };

            // 特殊規格：如果擁有特殊 GID 的權限，則認識為管理員
            if (queried.M_Group_User.Any(groupUser => groupUser.GID == JwtConstants.AdminGid))
                claims.Add(new Claim(ClaimTypes.Role, AuthorizeTypeSingletonFactory.Admin.GetRoleValue()));

            UserData_Login_Output_APIItem output = new UserData_Login_Output_APIItem
            {
                UID = queried.UID,
                Username = queried.UserName,
                JwtToken = JwtHelper.GenerateToken(JwtConstants.Secret, JwtConstants.ExpireMinutes, claims)
            };

            return GetResponseJson(output);
        }

        /// <summary>
        /// 更新使用者的登入日期時間。
        /// </summary>
        /// <param name="user">使用者資料</param>
        /// <returns>
        /// true：更新成功。<br/>
        /// false：更新失敗。
        /// </returns>
        private async Task<bool> UpdateUserLoginDate(UserData user)
        {
            bool result = true;

            try
            {
                user.LoginDate = DateTime.Now;
                await DC.SaveChangesStandardProcedureAsync(user.UID);
            }
            catch (Exception e)
            {
                AddError(LoginDateUpdateFailed(e));
                result = false;
            }

            return result;
        }

        #endregion

        #region Submit

        /// <summary>
        /// 更新使用者資料。<br/>
        /// 如果 LoginPassword 有輸入且與資料庫不同時，則會多驗證 OriginalPassword。<br/>
        /// Note 欄位無論有無輸入都會更新，因此呼叫者輸入時需要自行維持 Note 的內容。
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
            throw new NotSupportedException();
        }

        public Task<UserData> SubmitCreateData(UserData_Submit_Input_APIItem input)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region Submit - Edit

        public async Task<bool> SubmitEditValidateInput(UserData_Submit_Input_APIItem input)
        {
            bool isValid = input.StartValidate()
                .Validate(i => i.UID.IsAboveZero(), () => AddError(EmptyNotAllowed("使用者 ID")))
                .Validate(i => !i.Username.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("使用者名稱")))
                .Validate(i => !i.LoginAccount.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("使用者帳號")))
                .Validate(i => !i.LoginPassword.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("使用者密碼")))
                .Validate(i => i.LoginPassword.IsEncryptablePassword(), () => AddError(PasswordAlphanumericOnly))
                .Validate(i => i.DDID.IsAboveZero(), () => AddError(EmptyNotAllowed("部門 ID")))
                .Validate(i => i.GID.IsAboveZero(), () => AddError(EmptyNotAllowed("身分 ID")))
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
                        data.LoginPassword =
                input.LoginPassword.IsNullOrWhiteSpace()
                    ? data.LoginPassword
                    : EncryptPassword(input.LoginPassword);

            // Note 是可選欄位，因此呼叫者應該保持原始內容
            data.Note = input.Note;

            data.DDID = input.DDID;
            
            // 如果是管理員，才允許繼續更新後續的欄位
            if (!FilterStaticTools.HasRoleInRequest(HttpContext.Request, AuthorizeBy.Admin)) return;
            
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
                M_Group_User groupUser = new M_Group_User
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
            M_Group_User newGroupUser = groupUsers.First();
            newGroupUser.GID = input.GID;
        }

        #endregion


        #endregion

        #region 密碼驗證

        /// <summary>
        /// 針對使用者密碼進行加密。<br/>
        /// 當使用者密碼為空白、空格、null，或包含非英數字時，回傳 (false, null)。
        /// </summary>
        /// <param name="password">使用者密碼</param>
        /// <returns>result: 加密結果<br/>encryptedPassword: 加密後的字串</returns>
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
        /// 驗證輸入密碼是否與指定的加密字串相符。
        /// </summary>
        /// <param name="input">原始輸入的密碼</param>
        /// <param name="data">資料庫的已加密密碼</param>
        /// <returns>true：相符<br/>
        /// false：不相符，或密碼輸入包含半形英數字以外的字元
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
        public async Task<string> DeleteItem(int id, bool? deleteFlag)
        {
            return await _deleteItemHelper.DeleteItem(id, deleteFlag);
        }

        public IQueryable<UserData> DeleteItemQuery(int id)
        {
            return DC.UserData.Where(u => u.UID == id);
        }

        #endregion

        #region ChangeActive

        /// <summary>
        /// 啟用 / 停止使用者帳號。
        /// </summary>
        /// <param name="input">輸入資料。</param>
        /// <returns>通用訊息回傳格式。</returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin, RequirePrivilege.EditFlag)]
        public async Task<string> ChangeActive(UserData_ChangeActive_Input_APIItem input)
        {
            // 1. 驗證 input 的 TargetUid 格式正確
            // 2. 更新 UserData
            // 3. 回傳通用 Response
            await input.StartValidate(true)
                .Validate(i => i.ID.IsAboveZero(),
                    () => AddError(UpdateUidIncorrect))
                .Validate(i => i.ActiveFlag != null,
                    () => AddError(EmptyNotAllowed("ActiveFlag")))
                // ReSharper disable once PossibleInvalidOperationException
                .ValidateAsync(async i => await ChangeActiveFlagForUserData(input.ID, (bool)input.ActiveFlag),
                    e => AddError(e.Message));

            return GetResponseJson();
        }

        private async Task ChangeActiveFlagForUserData(int inputTargetUid, bool newValue)
        {
            UserData queried;

            try
            {
                queried = await GetUserDataById(inputTargetUid);
            }
            catch (Exception e)
            {
                throw new DataException(QueryFailed(e));
            }

            if (queried == null || queried.DeleteFlag)
                throw new NullReferenceException(UserDataNotFound);

            try
            {
                queried.ActiveFlag = newValue;
                await DC.SaveChangesStandardProcedureAsync(GetUid());
            }
            catch (Exception e)
            {
                throw new NullReferenceException(ChangeActiveUpdateFailed(e));
            }
        }

        private async Task<UserData> GetUserDataById(int uid) =>
            await DC.UserData.FirstOrDefaultAsync(u => u.UID == uid);

        #endregion

        #region UpdatePW

        /// <summary>
        /// 更新使用者密碼。
        /// </summary>
        /// <param name="input"><see cref="UserData_UpdatePW_Input_APIItem"/></param>
        /// <returns>通用回傳訊息格式</returns>
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin | AuthorizeBy.UserSelf, RequirePrivilege.EditFlag)]
        public async Task<string> UpdatePW(UserData_UpdatePW_Input_APIItem input)
        {
            // 1. 驗證。
            // |- a. 驗證所有輸入均有值
            // |- b. 驗證新密碼可加密
            // +- c. 成功更新資料庫
            bool isValid = input
                .StartValidate()
                .Validate(i => i.ID.IsAboveZero(), () => AddError(UpdateUidIncorrect))
                .Validate(i => !i.NewPassword.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("新密碼")))
                .Validate(i => i.NewPassword.IsEncryptablePassword(), () => AddError(UpdatePWPasswordNotEncryptable))
                .IsValid();

            // 只在輸入都驗證過後才允許更新 DB
            if (isValid)
            {
                await input
                    .StartValidate(true)
                    .ValidateAsync(async i => await UpdatePasswordForUserData(i.ID, i.NewPassword),
                        e => AddError(e.Message));
            }

            // 2. 回傳通用訊息格式。
            return GetResponseJson();
        }

        private async Task UpdatePasswordForUserData(int id, string inputPassword)
        {
            // 1. 查詢資料。無資料時拋錯
            UserData queried = await GetUserDataById(id);
            if (queried == null)
                throw new NullReferenceException(UserDataNotFound);

            // 2. 更新資料，更新失敗時拋錯
            try
            {
                queried.LoginPassword = EncryptPassword(inputPassword);
                await DC.SaveChangesStandardProcedureAsync(GetUid());
            }
            catch (Exception e)
            {
                throw new DbUpdateException(UpdatePWDbFailed(e));
            }
        }

        #endregion

        #region GetList

        /// <summary>
        /// 取得使用者列表。
        /// </summary>
        /// <param name="input">輸入資料。請參照 <see cref="UserData_GetList_Input_APIItem"/>。</param>
        /// <returns>回傳結果。請參照 <see cref="UserData_GetList_Output_APIItem"/>，以及 <see cref="UserData_GetList_Output_Row_APIItem"/>。</returns>
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
                .Include(u => u.DD)
                .ThenInclude(d => d.DC)
                .Include(u => u.M_Group_User)
                .ThenInclude(gu => gu.G)
                .AsQueryable();

            if (!input.Keyword.IsNullOrWhiteSpace())
                query = query.Where(u => u.UserName.Contains(input.Keyword));

            if (input.DCID.IsAboveZero())
                query = query.Where(u => u.DD.DCID == input.DCID);

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
                Department = entity.DD.TitleC,
                // 目前系統每個使用者只會有一個 Group
                Role = entity.M_Group_User.FirstOrDefault()?.G?.Title ?? ""
            });
        }

        #endregion

        #region GetInfoById

        /// <summary>
        /// 查詢單筆使用者資料。
        /// </summary>
        /// <param name="input">輸入資訊。請參照 <see cref="UserData_GetInfoById_Input_APIItem"/>。</param>
        /// <returns><see cref="UserData_GetInfoById_Output_APIItem"/></returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin | AuthorizeBy.UserSelf, RequirePrivilege.ShowFlag, "UID", null)]
        public async Task<string> GetInfoById(UserData_GetInfoById_Input_APIItem input)
        {
            UserData userData = null;

            bool isValid = await input.StartValidate(true)
                .Validate(i => i.UID.IsAboveZero(), () => AddError(GetUidIncorrect))
                .ValidateAsync(async i => userData = await GetMakeQuery(i), e => AddError(QueryFailed(e)))
                .Validate(i => userData != null, () => AddError(GetUserNotFound))
                .IsValid();

            return isValid ? GetResponseJson(UserDataToGetInfoByIdOutput(userData)) : GetResponseJson();
        }

        private static UserData_GetInfoById_Output_APIItem UserDataToGetInfoByIdOutput(UserData userData)
        {
            return new UserData_GetInfoById_Output_APIItem
            {
                UID = userData.UID,
                Username = userData.UserName,
                LoginAccount = userData.LoginAccount,
                LoginPassword = HSM.Des_1(userData.LoginPassword),
                DDID = userData.DDID,
                GID = userData.M_Group_User
                    .Where(gu => gu.G.ActiveFlag && !gu.G.DeleteFlag)
                    .OrderBy(gu => gu.GID)
                    .FirstOrDefault()?
                    .GID ?? 0,
                ActiveFlag = userData.ActiveFlag,
                Note = userData.Note
            };
        }

        private async Task<UserData> GetMakeQuery(UserData_GetInfoById_Input_APIItem input)
        {
            UserData userData = await DC.UserData
                .Include(u => u.M_Group_User)
                .ThenInclude(gu => gu.G)
                .FirstOrDefaultAsync(u => !u.DeleteFlag && u.UID == input.UID);
            return userData;
        }

        #endregion
    }
}