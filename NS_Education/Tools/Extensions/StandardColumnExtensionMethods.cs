using System;
using System.Linq;
using Microsoft.Ajax.Utilities;

namespace NS_Education.Tools.Extensions
{
    public static class StandardColumnExtensionMethods
    {
        /// <summary>
        /// 驗證輸入是否符合 ID 值的規則。
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>true：ID 正確。<br/>
        /// false：ID 錯誤。
        /// </returns>
        public static bool IsValidId(this int id)
        {
            return id > 0;
        }
        
        /// <summary>
        /// 驗證輸入是否符合 ID 值的規則。
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>true：ID 正確。<br/>
        /// false：ID 錯誤。
        /// </returns>
        public static bool IsValidId(this int? id)
        {
            return id > 0;
        }
        
        /// <summary>
        /// 驗證輸入是否符合 ID 值的規則，並允許 0（通常為新增模式）。
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>true：ID 正確。<br/>
        /// false：ID 錯誤。
        /// </returns>
        public static bool IsValidIdOrZero(this int? id)
        {
            return id >= 0;
        }

        /// <summary>
        /// 驗證一組字串是否符合本專案目前支援的密碼格式。<br/>
        /// 目前只支援英數字。
        /// </summary>
        /// <param name="password">欲驗證的字串</param>
        /// <returns>
        /// true：符合<br/>
        /// false：不符合<br/>
        /// </returns>
        public static bool IsEncryptablePassword(this string password)
        {
            return !password.IsNullOrWhiteSpace() && password.All(Char.IsLetterOrDigit);
        }
        
        /// <summary>
        /// 將日期轉換為 yyyy/MM/dd HH:mm 格式
        /// </summary>
        /// <param name="datetime">日期（可包含時間）。</param>
        /// <returns>格式化的日期字串</returns>
        public static string ToFormattedString(this DateTime datetime)
        {
            return datetime.ToString("yyyy/MM/dd HH:mm");
        }
    }
}