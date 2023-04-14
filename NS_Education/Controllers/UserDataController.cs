using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http.Results;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using Microsoft.EntityFrameworkCore;
using NS_Education.Controllers.BaseClass;
using NS_Education.Models.APIItems.UserData.ChangeActive;
using NS_Education.Models.APIItems.UserData.DeleteItem;
using NS_Education.Models.APIItems.UserData.Login;
using NS_Education.Models.APIItems.UserData.Submit;
using NS_Education.Models.APIItems.UserData.UpdatePW;
using NS_Education.Models.Entities;
using NS_Education.Tools.BeingValidated;
using NS_Education.Tools.Encryption;
using NS_Education.Tools.Extensions;
using NS_Education.Tools.Filters.JwtAuthFilter;
using NS_Education.Tools.Filters.JwtAuthFilter.AuthorizeType;
using NS_Education.Tools.Filters.JwtAuthFilter.PrivilegeType;
using NS_Education.Variables;

namespace NS_Education.Controllers
{
    public class UserDataController : PublicClass
    {
        #region 錯誤訊息 - 通用
        /// <summary>
        /// 回傳「請填入{columnName}！」
        /// </summary>
        /// <param name="columnName">欄位名稱</param>
        /// <returns>錯誤訊息字串</returns>
        private static string EmptyNotAllowed(string columnName)
            => $"請填入{columnName}！";
        
        private const string UpdateUidIncorrect = "未提供欲修改的 UID 或格式不正確！";
        private const string UserDataNotFound = "這筆使用者不存在或已被刪除！";
        #endregion

        #region 錯誤訊息 - 註冊/更新
        private const string SubmitOriginalPasswordEmptyOrIncorrect = "原密碼未輸入或不正確！";
        private const string SubmitPasswordAlphanumericOnly = "使用者密碼只允許半形英文字母、數字！";
        private const string SubmitUidIncorrect = "缺少 UID，無法寫入！";
        #endregion

        #region 錯誤訊息 - 登入
        private const string LoginAccountNotFound = "查無此使用者帳號，請重新確認！";
        private const string LoginPasswordIncorrect = "使用者密碼錯誤！";
        private static string LoginDateUpdateFailed(Exception e)
            => $"上次登入時間更新失敗，錯誤訊息：{e.Message}！";
        #endregion

        #region 錯誤訊息 - 刪除

        private const string DeleteItemOperatorUidIncorrect = "未提供操作者的 UID，無法寫入！";
        private const string DeleteItemTargetUidIncorrect = "未提供欲刪除的 UID 或格式不正確！";
        private const string DeleteItemTargetUidNotFound = "查無對應的欲刪除 UID，請檢查輸入是否正確！";
        private const string DeleteItemTargetAlreadyDeleted = "指定使用者已為刪除狀態！";
        private static string DeleteItemFailed(Exception e)
            => $"刪除使用者時失敗，錯誤訊息：{e.Message}！";
        
        #endregion
        
        #region 錯誤訊息 - 啟用/停用

        private static string ChangeActiveUpdateFailed(Exception e)
        {
            return $"更新 DB 時出錯，請確認伺服器狀態：{e.Message}";
        }

        private static string ChangeActiveQueryFailed(Exception e)
        {
            return $"查詢 DB 時出錯，請確認伺服器狀態：{e.Message}";
        }
        
        #endregion
        
        #region 錯誤訊息 - 更新密碼
        
        private static string UpdatePWDbFailed(Exception e) => $"更新密碼時失敗，請確認伺服器狀態：{e.Message}！";
        private const string UpdatePWPasswordNotEncryptable = "密碼只允許英數字！";
            
        #endregion
        
        #region SignUp
        /// <summary>
        /// 註冊使用者資料。過程中會驗證使用者輸入，並在回傳時一併報錯。<br/>
        /// 如果過程驗證都通過，才寫入資料庫。
        /// </summary>
        /// <param name="input">輸入資料</param>
        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin)]
        public async Task<string> SignUp(UserData_Submit_Input_APIItem input)
        {
            InitializeResponse();
            
            // TODO: 引用靜態參數檔，完整驗證使用者密碼

            // sanitize
            if (!input.UID.IsCorrectUid())
                AddError(SubmitUidIncorrect);
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
                    AddError(SubmitPasswordAlphanumericOnly);
                    // 這裡不做提早返回，方便一次顯示更多錯誤訊息給使用者
                }
            }

            // create UserData object, validate the columns along
            // TODO: 引用靜態參數檔，完整驗證使用者欄位
            UserData newUser = new UserData
            {
                UserName = input.Username.ExecuteIfNullOrWhiteSpace(() => AddError(EmptyNotAllowed("使用者名稱"))),
                LoginAccount = input.LoginAccount.ExecuteIfNullOrWhiteSpace(() => AddError(EmptyNotAllowed("使用者帳號"))),
                LoginPassword = input.LoginPassword,
                Note = input.Note,
                ActiveFlag = true,
                DeleteFlag = false,
                CreDate = DateTime.Now,
                CreUID = input.UID,
                UpdDate = DateTime.Now,
                UpdUID = 0,
                LoginDate = DateTime.Now,
                DDID = input.DDID
            };

            // doesn't write to db if any error raised
            // For postman testing: 若備註欄為特殊值時，不真正寫入資料。
            if (HasError() || IsATestRegister(input)) return GetResponseJson();
            
            await DC.UserData.AddAsync(newUser);
            await DC.SaveChangesAsync();

            return GetResponseJson();
        }

        // TODO: 在確保單元測試方式之後，將此處邏輯刪除。
        private static bool IsATestRegister(UserData_Submit_Input_APIItem input)
        {
            return input.Note?.ToLower().Equals("newregistertest") ?? false;
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
                ? await DC.UserData.FirstOrDefaultAsync(u => u.LoginAccount == input.LoginAccount)
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
            UserData_Login_Output_APIItem output = new UserData_Login_Output_APIItem
            {
                // queried 已經在上面驗證為非 null。目前專案使用 C# 版本不支援 !，所以以此代替。 
                // ReSharper disable once PossibleNullReferenceException
                UID = queried.UID,
                Username = queried.UserName,
                JwtToken = JwtHelper.GenerateToken(JwtConstants.Secret, JwtConstants.ExpireMinutes, new []
                {
                    new Claim(JwtConstants.UidClaimType, queried.UID.ToString()),
                    // TODO: 等權限群組相關設計確認後，這裡加上 Admin 判定
                    new Claim(ClaimTypes.Role, AuthorizeTypeSingletonFactory.User.GetRoleValue())
                })
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
                await DC.SaveChangesAsync();
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
        [JwtAuthFilter(AuthorizeBy.Admin | AuthorizeBy.UserSelf, RequirePrivilege.EditFlag, nameof(UserData_Submit_Input_APIItem.UID))]
        public async Task<string> Submit(UserData_Submit_Input_APIItem input)
        {
            InitializeResponse();

            // sanitize
            if (!input.UID.IsCorrectUid())
                AddError(SubmitUidIncorrect);
            
            if (input.LoginAccount.IsNullOrWhiteSpace())
                AddError(EmptyNotAllowed("使用者帳號"));
            
            // 如果輸入就錯了，提早返回。
            if (HasError())
                return GetResponseJson();
            
            UserData original = await DC.UserData.FirstOrDefaultAsync(u => u.LoginAccount == input.LoginAccount);
            
            if (original == null)
            {
                AddError(LoginAccountNotFound);
                return GetResponseJson();
            }

            // 更新資料
            // TODO: 引用靜態參數檔，完整驗證使用者欄位 
            try
            {
                // 只在欄位有輸入任何資料時，才更新對應欄位
                original.UserName = input.Username.IsNullOrWhiteSpace() ? original.UserName : input.Username;
                original.LoginAccount =
                    input.LoginAccount.IsNullOrWhiteSpace() ? original.LoginAccount : input.LoginAccount;
                original.LoginPassword =
                    input.LoginPassword.IsNullOrWhiteSpace()
                        ? original.LoginPassword
                        : EncryptPassword(input.LoginPassword);
                
                // Note 是可選欄位，因此呼叫者應該保持原始內容
                original.Note = input.Note;
                
                original.UpdDate = DateTime.Now;
                original.UpdUID = input.UID;

                original.DDID = input.DDID;
            }
            catch (ValidationException)
            {
                // 密碼錯誤時報錯並提早返回。
                AddError(SubmitPasswordAlphanumericOnly);
                return GetResponseJson();
            }

            // TODO: 在確保單元測試方式之後，將此處邏輯刪除。
            if (IsATestUpdate(input))
                return GetResponseJson();

            if (HasError())
                return GetResponseJson();
            
            await DC.SaveChangesAsync();
            return GetResponseJson();
        }

        // TODO: 在確保單元測試方式之後，將此處邏輯刪除。
        private static bool IsATestUpdate(UserData_Submit_Input_APIItem input)
        {
            return input.Note?.ToLower().Equals("updatetest") ?? false;
        }
        
        /// <summary>
        /// 回傳輸入的密碼是否與資料庫密碼不同（表示使用者在改密碼）。
        /// </summary>
        /// <param name="inputPassword">原始輸入的 LoginPassword</param>
        /// <param name="dataPassword">資料的已加密密碼</param>
        /// <returns>true：符合<br/>
        /// false：不符合
        /// </returns>
        private static bool IsChangingPassword(string inputPassword, string dataPassword)
        {
            return !inputPassword.IsNullOrWhiteSpace() && !ValidatePassword(inputPassword, dataPassword);
        }

        /// <summary>
        /// 回傳輸入的原始密碼是否符合資料庫密碼。
        /// </summary>
        /// <param name="inputOriginalPassword">原始輸入的 OriginalPassword</param>
        /// <param name="dataPassword">資料庫的已加密密碼</param>
        /// <returns>true：符合<br/>
        /// false：不符合
        /// </returns>
        private static bool HasCorrectOriginalPassword(string inputOriginalPassword, string dataPassword)
        {
            return ValidatePassword(inputOriginalPassword, dataPassword);
        }
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
                throw new ValidationException(SubmitPasswordAlphanumericOnly);
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

        /// <summary>
        /// 將指定的 UID 的使用者資料改為刪除狀態。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>通用回傳訊息格式</returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin)]
        public async Task<string> DeleteItem(UserData_DeleteItem_Input_APIItem input)
        {
            // 驗證輸入。
            // 1. 操作者 UID 是否正確。
            // 2. 刪除對象 UID 是否正確。
            bool isInputValid = input.StartValidate(true)
                .Validate(i => i.OperatorUID.IsCorrectUid(), () => AddError(DeleteItemOperatorUidIncorrect))
                .Validate(i => i.TargetUID.IsCorrectUid(), () => AddError(DeleteItemTargetUidIncorrect)).IsValid();

            if (!isInputValid)
                return GetResponseJson();

            // 查詢資料並驗證。
            UserData queried = DC.UserData.FirstOrDefault(u => u.UID == input.TargetUID);
            // 1. 進行查詢後，是否有查到資料。
            // 2. 該筆資料是否並非刪除狀態。
            bool isDataValid = queried.StartValidate(true)
                .Validate(q => q != null, () => AddError(DeleteItemTargetUidNotFound))
                .Validate(q => q.DeleteFlag == false, () => AddError(DeleteItemTargetAlreadyDeleted)).IsValid();
            
            // 資料未通過驗證時，提早返回。
            if (!isDataValid)
                return GetResponseJson();

            try
            {
                // 更新資料。
                // ReSharper disable once PossibleNullReferenceException
                queried.DeleteFlag = true;
                queried.UpdDate = DateTime.Now;
                queried.UpdUID = input.OperatorUID;
                await DC.SaveChangesAsync();
            }
            catch (Exception e)
            {
                AddError(DeleteItemFailed(e));
            }

            return GetResponseJson();
        }

        #endregion

        #region ChangeActive
        /// <summary>
        /// 啟用 / 停止使用者帳號。
        /// </summary>
        /// <param name="input">輸入資料。</param>
        /// <returns>通用訊息回傳格式。</returns>
        [HttpGet]
        [JwtAuthFilter(AuthorizeBy.Admin)]
        public async Task<string> ChangeActive(UserData_ChangeActive_Input_APIItem input)
        {
            // 1. 驗證 input 的 TargetUid 格式正確
            // 2. 更新 UserData
            // 3. 回傳通用 Response
            await input.StartValidate(true)
                .Validate(i => i.ID.IsCorrectUid(),
                    onFail: () => AddError(UpdateUidIncorrect))
                .ValidateAsync(async i => await ChangeActiveFlagForUserData(input.ID, input.ActiveFlag),
                    onException: e => AddError(e.Message));

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
                throw new DataException(ChangeActiveQueryFailed(e));
            }

            if (queried == null || queried.DeleteFlag)
                throw new NullReferenceException(UserDataNotFound);

            try
            {
                queried.ActiveFlag = newValue;
                await DC.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new NullReferenceException(ChangeActiveUpdateFailed(e));
            }
        }

        private async Task<UserData> GetUserDataById(int uid) => await DC.UserData.FirstOrDefaultAsync(u => u.UID == uid);

        #endregion

        #region UpdatePW

        [HttpPost]
        [JwtAuthFilter(AuthorizeBy.Admin | AuthorizeBy.UserSelf, RequirePrivilege.EditFlag)]
        public async Task<string> UpdatePW(UserData_UpdatePW_Input_APIItem input)
        {
            // 1. 驗證。
            // |- a. 驗證所有輸入均有值
            // |- b. 驗證新密碼可加密
            // +- c. 成功更新資料庫
            await input.StartValidate()
                .Validate(i => i.ID.IsCorrectUid(), () => AddError(UpdateUidIncorrect))
                .Validate(i => !i.NewPassword.IsNullOrWhiteSpace(), () => AddError(EmptyNotAllowed("新密碼")))
                .Validate(i => i.NewPassword.IsEncryptablePassword(), () => AddError(UpdatePWPasswordNotEncryptable))
                .ValidateAsync(async i => await UpdatePasswordForUserData(i.ID, i.NewPassword),
                    onException: e => AddError(e.Message));
            
            // 2. 回傳通用訊息格式。
            return GetResponseJson();
        }

        public async Task UpdatePasswordForUserData(int id, string inputPassword)
        {
            // 1. 查詢資料。無資料時拋錯
            UserData queried = await GetUserDataById(id);
            if (queried == null)
                throw new NullReferenceException(UserDataNotFound);
            
            // 2. 更新資料，更新失敗時拋錯
            try
            {
                queried.LoginPassword = EncryptPassword(inputPassword);
                await DC.SaveChangesAsync();
            }
            catch (Exception e)
            {
                throw new DbUpdateException(UpdatePWDbFailed(e));
            }
        }
        
        #endregion
        
        #region GetList

        #endregion

        #region GetInfoById

        

        #endregion
    }
}