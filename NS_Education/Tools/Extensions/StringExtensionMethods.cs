﻿using System;

namespace NS_Education.Tools.Extensions
{
    public static class StringExtensionMethods
    {
        public static string ExecuteIfNullOrWhiteSpace(this string s, Action func)
        {
            if (String.IsNullOrWhiteSpace(s))
                func.Invoke();

            return s;
        }

        public static string ExecuteIf(this string s, Predicate<string> predicate, Action func)
        {
            if (predicate.Invoke(s))
                func.Invoke();

            return s;
        }

        public static bool HasContent(this string s)
        {
            return !String.IsNullOrWhiteSpace(s);
        }

        public static bool HasLengthBetween(this string s, int min, int max)
        {
            if (s == null && min <= 0)
                return true;

            return s != null && s.Length.IsInBetween(min, max);
        }

        public static bool IsNullOrWhiteSpace(this string s)
        {
            return String.IsNullOrWhiteSpace(s);
        }

        public static string SanitizeForResponseStatusMessage(this string s)
        {
            return s.Replace("\r", "").Replace("\n", "");
        }

        /// <summary>
        /// 在字串前後加上全形括號。
        /// </summary>
        /// <param name="s">要處理的字串</param>
        /// <returns>
        /// 字串有內容時：首尾加上 「 符號與 」 符號的字串。<br/>
        /// 字串為 null、空或空格時：原字串。
        /// </returns>
        public static string UnicodeQuote(this string s)
        {
            if (s.IsNullOrWhiteSpace())
                return s;

            if (!s.StartsWith("「"))
                s = "「" + s;

            if (!s.EndsWith("」"))
                s += "」";

            return s;
        }

        /// <summary>
        /// 把格式 HH24:mm 的字串轉換成 TimeSpan。
        /// </summary>
        /// <param name="s">HH24:mm</param>
        /// <param name="ts">
        /// 轉換成功時：時間<br/>
        /// 失敗時：預設值
        /// </param>
        /// <returns>轉換成功或失敗</returns>
        public static bool TryParseTimeSpan(this string s, out TimeSpan ts)
        {
            ts = default;
            if (!s.HasLengthBetween(5, 5))
                return false;

            string[] splits = s.Split(':');

            if (splits.Length != 2)
                return false;

            ts = new TimeSpan(Convert.ToInt32(splits[0]), Convert.ToInt32(splits[1]), 0);
            return true;
        }
    }
}