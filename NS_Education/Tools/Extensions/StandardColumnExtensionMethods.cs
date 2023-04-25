using System;
using System.Globalization;
using System.Linq;
using NS_Education.Variables;

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
        public static bool IsValidIdOrZero(this int id)
        {
            return id >= 0;
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
        /// <returns>格式化的日期時間字串</returns>
        public static string ToFormattedStringDateTime(this DateTime datetime)
        {
            return datetime.ToString(IoConstants.DateTimeFormat);
        }
        
        /// <summary>
        /// 將日期轉換為 yyyy/MM/dd 格式
        /// </summary>
        /// <param name="datetime">日期。</param>
        /// <returns>格式化的日期字串</returns>
        public static string ToFormattedStringDate(this DateTime datetime)
        {
            return datetime.ToString(IoConstants.DateFormat);
        }

        /// <summary>
        /// 嘗試將字串轉換成 DateTime，成功時 result 將帶入轉換結果。
        /// </summary>
        /// <param name="s">字串</param>
        /// <param name="result">
        /// 轉換成功時：DateTime 結果<br/>
        /// 轉換失敗時：欲設的 DateTime 值
        /// </param>
        /// <returns>
        /// true：轉換成功<br/>
        /// false：轉換失敗
        /// </returns>
        public static bool TryParseDateTime(this string s, out DateTime result)
        {
            result = default;
            if (s == null)
                return false;
            
            s = s.Trim();

            if (s.Length == IoConstants.DateTimeFormat.Length)
                return DateTime.TryParseExact(s, IoConstants.DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out result);
            if (s.Length == IoConstants.DateFormat.Length)
                return DateTime.TryParseExact(s, IoConstants.DateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out result);
            
            return false;
        }

        /// <summary>
        /// 接受兩個整數，轉換成 00:00 的格式。
        /// </summary>
        /// <param name="tuple">傳入兩個整數的格式</param>
        /// <returns>00:00 格式的字串</returns>
        public static string ToFormattedHourAndMinute(this (int hour, int minute) tuple)
        {
            return $"{tuple.hour.ToString().PadLeft(2, '0')}:{tuple.minute.ToString().PadLeft(2, '0')}";
        }
    }
}