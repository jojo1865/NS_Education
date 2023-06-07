using System;

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
    }
}