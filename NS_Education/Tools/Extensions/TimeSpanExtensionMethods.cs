using System;

namespace NS_Education.Tools.Extensions
{
    public static class TimeSpanExtensionMethods
    {
        public static string ToFormattedStringTime(this TimeSpan? timeSpan)
        {
            return timeSpan?.ToString(@"hh\:mm");
        }
    }
}