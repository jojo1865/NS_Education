using System;
using System.Collections.Generic;
using System.Linq;

namespace NS_Education.Tools.Extensions
{
    public static class DateTimeExtension
    {
        public static IEnumerable<DateTime> Range(this DateTime start, DateTime end)
        {
            start = start.Date;
            end = end.Date;

            if (end < start)
                (start, end) = (end, start);

            return Enumerable.Range(0, (end - start).Days)
                .Select(i => start.AddDays(i))
                .ToArray();
        }
    }
}