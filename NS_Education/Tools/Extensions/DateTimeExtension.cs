using System;
using System.Collections.Generic;
using System.Linq;

namespace NS_Education.Tools.Extensions
{
    public static class DateTimeExtension
    {
        public static string FormatAsRocYyyMmDd(this DateTime dt)
        {
            System.Globalization.TaiwanCalendar tc = new System.Globalization.TaiwanCalendar();

            return $"{tc.GetYear(dt)}/{tc.GetMonth(dt)}/{tc.GetDayOfMonth(dt)}";
        }

        public static IEnumerable<DateTime> Range(this DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;

            if (end < start)
                (start, end) = (end, start);

            return Enumerable.Range(0, (end - start).Days + 1)
                .Select(i => start.AddDays(i))
                .ToArray();
        }
    }
}