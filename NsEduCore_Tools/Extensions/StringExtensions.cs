using System;

namespace NsEduCore_Tools.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// 檢查字串是否為 null，或為空，或只包含空格。
        /// </summary>
        /// <param name="s">對象字串</param>
        /// <returns>
        /// true：字串為 null，或為空，或只包含空格。<br/>
        /// false：字串包含內容。
        /// </returns>
        public static bool IsNullOrWhitespace(this string s)
            => String.IsNullOrWhiteSpace(s);
    }
}