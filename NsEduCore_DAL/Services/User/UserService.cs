using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NsEduCore_DAL.Models;
using NsEduCore_DAL.Models.Data;
using NsEduCore_Tools.Encryption;
using NsEduCore_Tools.Extensions;

namespace NsEduCore_DAL.Services.User
{
    public class UserService : IUserService
    {
        private readonly NsDataContext _context;
        
        private const string EncryptPasswordAlphanumericOnly = "使用者密碼只允許半形英文字母、數字！";

        public UserService(NsDataContext nsDataContext)
        {
            _context = nsDataContext;
        }
        
        public async Task<User> SelectByLoginAccount(string loggingAccount)
        {
            UserData queried = await _context.UserData.FirstOrDefaultAsync(u => u.LoginAccount == loggingAccount);
            return queried == null ? null : ToUser(queried);
        }
        
        public async Task<bool> ValidateLoginPassword(User user, string inputPassword)
        {
            return await Task.Run(() => ValidatePassword(inputPassword, user.UserData.LoginPassword));
        }

        private static User ToUser(UserData queried)
        {
            User result = new User
            {
                UID = queried.UID,
                UserName = queried.UserName,
                LoginAccount = queried.LoginAccount,
                Note = queried.Note,
                ActiveFlag = queried.ActiveFlag,
                DeleteFlag = queried.DeleteFlag,
                CreDate = queried.CreDate,
                CreUID = queried.CreUID,
                UpdUID = queried.UpdUID,
                UpdDate = queried.UpdDate,
                LoginDate = queried.LoginDate,
                UserData = queried
            };
            return result;
        }

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
            if (password.Any(c => !Char.IsLetterOrDigit(c)))
                throw new ValidationException(EncryptPasswordAlphanumericOnly);

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
            if (input.IsNullOrWhitespace())
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
        
        public async Task UpdateLoginDate(User user)
        {
            user.UserData.LoginDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}