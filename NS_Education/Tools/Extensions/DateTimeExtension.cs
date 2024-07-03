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

        /// <summary>
        /// 建立 JWT 時，從 start 起算，取得 JWT 的有效期限。
        /// </summary>
        /// <param name="start">計算的開始時間</param>
        /// <returns>過期時間</returns>
        public static DateTime GetNextJwtExpireDateTime(this DateTime start)
        {
            // 假設從 start 時發行 JWT，回傳此 JWT 的有效期限
            // 目前系統的有效時間是 (登入瞬間, 登入當天晚上 23:59:59)
            // 避免在白天時過期而導致操作中斷

            return start.Date.AddDays(1).AddSeconds(-1);
        }

        /// <summary>
        /// 傳入 end，計算從這個 DateTime 到 end 還有多少分鐘。
        /// </summary>
        /// <param name="start">開始時間</param>
        /// <param name="end">結束時間</param>
        /// <returns>剩餘分鐘數</returns>
        public static int MinutesUntil(this DateTime start, DateTime end)
        {
            return Convert.ToInt32((end - start).TotalMinutes);
        }
    }
}