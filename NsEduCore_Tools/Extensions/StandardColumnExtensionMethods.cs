using System;
using Microsoft.VisualBasic;

namespace NsEduCore_Tools.Extensions
{
    public static class StandardColumnExtensionMethods
    {
        /// <summary>
        /// 驗證 ID 欄位是否符合格式。<br/>
        /// 要求 id 不是 null 且大於 0。
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>true：ID 正確。<br/>
        /// false：ID 錯誤。
        /// </returns>
        public static bool IsValidId(this int? id)
        {
            // 檢查非 null 且大於 0
            return id is > 0;
        }
        
        /// <summary>
        /// 驗證 ID 欄位是否符合格式。<br/>
        /// 要求 id 大於 0。
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>true：ID 正確。<br/>
        /// false：ID 錯誤。
        /// </returns>
        public static bool IsValidId(this int id)
        {
            // 檢查大於 0
            return id > 0;
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