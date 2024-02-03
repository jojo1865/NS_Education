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

        public static string FormatAsRocYyyMmDdWeekDay(this DateTime dt)
        {
            string[] weekDayChinese = { "日", "一", "二", "三", "四", "五", "六" };
            string weekDay = weekDayChinese[(int)dt.DayOfWeek];

            return dt.FormatAsRocYyyMmDd() + $"(週{weekDay})";
        }

        public static IEnumerable<DateTime> DayRange(this DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;

            if (end < start)
                (start, end) = (end, start);

            return Enumerable.Range(0, (end - start).Days + 1)
                .Select(i => start.AddDays(i))
                .ToArray();
        }

        public static IEnumerable<DateTime> MonthRange(this DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;

            if (end < start)
                (start, end) = (end, start);

            return Enumerable.Range(0, start.TotalMonths(end))
                .Select(i => start.AddMonths(i))
                .ToArray();
        }

        public static int TotalMonths(this DateTime start, DateTime end)
        {
            if (start > end)
                (start, end) = (end, start);

            return (end.Year * 12 + end.Month) - (start.Year * 12 + start.Month) + 1;
        }
    }
}