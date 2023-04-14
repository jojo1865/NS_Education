using System;
using System.Linq;
using Microsoft.Ajax.Utilities;

namespace NS_Education.Tools.Extensions
{
    public static class StandardColumnExtensionMethods
    {
        /// <summary>
        /// 驗證 UID 是否正確。
        /// </summary>
        /// <param name="uid">uid</param>
        /// <returns>true：UID 正確。<br/>
        /// false：UID 錯誤。
        /// </returns>
        public static bool IsCorrectUid(this int uid)
        {
            // TODO: 加上驗證資料庫？
            return uid > 0;
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
    }
}