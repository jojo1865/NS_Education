using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using NS_Education.Models;
using NS_Education.Tools;

namespace NS_Education.Controllers
{
    public class UserController : PublicClass
    {
        /// <summary>
        /// 回傳「請填入{columnName}！」
        /// </summary>
        /// <param name="columnName">欄位名稱</param>
        /// <returns>錯誤訊息字串</returns>
        private static string EmptyNotAllowed(string columnName)
            => $"請填入{columnName}！";

        private static string OriginalPasswordEmptyOrIncorrect => "原密碼未輸入或不正確！";
        private static string PasswordAlphanumericOnly => "使用者密碼只允許半形英文字母、數字！";
        private static string UidIncorrect => "缺少 UID，無法寫入！";
        
        /// <summary>
        /// 註冊、更新使用者資料。<br/>
        /// 會依據 LoginAccount 是否已經存在於資料庫，判定是註冊還是更新。
        /// </summary>
        /// <param name="input">輸入資料。如果為更新，且 LoginPassword 如有輸入且與資料庫不同時，則會多驗證 OriginalPassword。</param>
        /// <returns>通用回傳訊息格式。當註冊時 Note 欄位以外有任何空白，或密碼驗證錯誤時，回傳錯誤。</returns>
        [HttpPost]
        public string Submit(UserData_Submit_Input_APIItem input)
        {
            UserData checkedUser = DC.UserData.FirstOrDefault(u => u.LoginAccount == input.LoginAccount);
            // TODO: 在確保單元測試方式之後，將此處的測試相關邏輯刪除。
            bool isRegister = !IsATestUpdate(input) && checkedUser == null || IsATestRegister(input);
            
            if (isRegister)
                Register(input);
            else
                Update(input, checkedUser);

            return GetResponseJson();
        }

        /// <summary>
        /// 註冊使用者資料。過程中會驗證使用者輸入，並在回傳時一併報錯。<br/>
        /// 如果過程驗證都通過，才寫入資料庫。
        /// </summary>
        /// <param name="input">輸入資料</param>
        private void Register(UserData_Submit_Input_APIItem input)
        {
            InitializeResponse();
            
            // TODO: 引用靜態參數檔，完整驗證使用者密碼

            // sanitize
            if (input.UID.IsIncorrectUid())
                AddError(UidIncorrect);
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
                LoginDate = DateTime.Now
            };

            // doesn't write to db if any error raised
            if (HasError())
                return;
            
            // For postman testing: 若備註欄為特殊值時，不真正寫入資料。
            // TODO: 在確保單元測試方式之後，將此處邏輯刪除。
            if (IsATestRegister(input))
                return;
            
            DC.UserData.InsertOnSubmit(newUser);
            DC.SubmitChanges();
        }

        // TODO: 在確保單元測試方式之後，將此處邏輯刪除。
        private static bool IsATestRegister(UserData_Submit_Input_APIItem input)
        {
            return input.Note.ToLower().Equals("newregistertest");
        }

        /// <summary>
        /// 更新使用者資料。<br/>
        /// 如果 LoginPassword 有輸入且與資料庫不同時，則會多驗證 OriginalPassword。<br/>
        /// Note 欄位無論有無輸入都會更新，因此呼叫者輸入時需要自行維持 Note 的內容。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <param name="original">呼叫此方法前，應已先取得原始使用者資料，並在此傳入。</param>
        private void Update(UserData_Submit_Input_APIItem input, UserData original)
        {
            InitializeResponse();

            // sanitize
            if (input.UID.IsIncorrectUid())
                AddError(UidIncorrect);
            
            // 若密碼有輸入且有變時，驗證 OriginalPassword。
            if (IsChangingPassword(input.LoginPassword, original.LoginPassword))
            {
                // 如果輸入的 OriginalPassword 和資料庫密碼不相符，跳錯。
                if (!HasCorrectOriginalPassword(input.OriginalPassword, original.LoginPassword))
                {
                    AddError(OriginalPasswordEmptyOrIncorrect);
                    // 這裡不做提早返回，方便一次顯示更多錯誤訊息給使用者
                }
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
            }
            catch (ValidationException)
            {
                // 密碼錯誤時報錯並提早返回。
                AddError(PasswordAlphanumericOnly);
                return;
            }

            // TODO: 在確保單元測試方式之後，將此處邏輯刪除。
            if (IsATestUpdate(input))
                return;

            if (HasError())
                return;
            
            DC.SubmitChanges();
        }

        // TODO: 在確保單元測試方式之後，將此處邏輯刪除。
        private static bool IsATestUpdate(UserData_Submit_Input_APIItem input)
        {
            return input.Note.ToLower().Equals("updatetest");
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
            if (password.Any(c => !Char.IsLetterOrDigit(c)))
                throw new ValidationException(PasswordAlphanumericOnly);

            return HSM.Enc_1(password);
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
            if (input.IsNullOrWhiteSpace())
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
    }
}