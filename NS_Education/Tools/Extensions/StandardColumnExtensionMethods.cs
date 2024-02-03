using System;
using System.Globalization;
using System.Linq;
using NS_Education.Variables;

namespace NS_Education.Tools.Extensions
{
    public static class StandardColumnExtensionMethods
    {
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
        /// <param name="type">（可選）允許轉換的格式。忽略時，皆允許。</param>
        /// <returns>
        /// true：轉換成功<br/>
        /// false：轉換失敗
        /// </returns>
        public static bool TryParseDateTime(this string s, out DateTime result,
            DateTimeParseType type = DateTimeParseType.Date | DateTimeParseType.DateTime)
        {
            result = default;
            if (s == null)
                return false;

            s = s.Trim();

            if (type.HasFlag(DateTimeParseType.DateTime) && s.Length == IoConstants.DateTimeFormat.Length)
                return DateTime.TryParseExact(s, IoConstants.DateTimeFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out result);
            if (type.HasFlag(DateTimeParseType.Date) && s.Length == IoConstants.DateFormat.Length)
                return DateTime.TryParseExact(s, IoConstants.DateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out result);

            return false;
        }

        /// <summary>
        /// 將字串轉換成 DateTime，無論成功或失敗，回傳轉換結果。
        /// </summary>
        /// <param name="s">字串</param>
        /// <param name="type">（可選）允許轉換的格式。忽略時，皆允許。</param>
        /// <returns>
        /// 轉換成功時：對象 DateTime<br/>
        /// 轉換失敗時：DateTime 的預設值
        /// </returns>
        public static DateTime ParseDateTime(this string s,
            DateTimeParseType type = DateTimeParseType.Date | DateTimeParseType.DateTime | DateTimeParseType.YearMonth)
        {
            DateTime result = default;

            if (s == null)
                return default;

            s = s.Trim();

            if (type.HasFlag(DateTimeParseType.DateTime) && s.Length == IoConstants.DateTimeFormat.Length)
                DateTime.TryParseExact(s, IoConstants.DateTimeFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out result);
            if (type.HasFlag(DateTimeParseType.Date) && s.Length == IoConstants.DateFormat.Length)
                DateTime.TryParseExact(s, IoConstants.DateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out result);
            if (type.HasFlag(DateTimeParseType.YearMonth) && s.Length == IoConstants.YearMonthFormat.Length)
                DateTime.TryParseExact(s, IoConstants.YearMonthFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out result);

            return result;
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

        /// <summary>
        /// 接受兩個整數，轉換成 00:00 的格式。
        /// </summary>
        /// <param name="hour">代表小時的整數</param>
        /// <param name="minute">代表分鐘的整數</param>
        /// <returns>00:00 格式的字串</returns>
        public static string ToFormattedHourAndMinute(int hour, int minute)
        {
            return $"{hour.ToString().PadLeft(2, '0')}:{minute.ToString().PadLeft(2, '0')}";
        }

        /// <summary>
        /// 接受兩組開始時間與結束時間，計算兩者間差異，轉換成 n 小時 m 分鐘的格式。不支援跨日。
        /// </summary>
        /// <param name="startTime">起始時間</param>
        /// <param name="endTime">結束時間</param>
        /// <returns>「n小時m分鐘」格式的字串</returns>
        public static string FormatTimeSpanUntil(this (int hour, int minute) startTime, (int hour, int minute) endTime)
        {
            // 計算 GetTimespan
            // 將兩種時間都換算成總分鐘數, 然後再相減
            int timeDiff = startTime.GetMinutesUntil(endTime);
            // 如果是負數的情況，當成 0 輸出
            timeDiff = Math.Max(timeDiff, 0);
            // 生成結果字串
            string result = "";
            if (timeDiff >= 60)
                result += $"{timeDiff / 60}小時";
            result += $"{timeDiff % 60}分鐘";
            return result;
        }

        /// <summary>
        /// 接受兩組開始時間與結束時間，計算兩者間差異分鐘數，不支援跨日。
        /// </summary>
        /// <param name="startTime">起始時間</param>
        /// <param name="endTime">結束時間</param>
        /// <returns>分鐘數</returns>
        public static int GetMinutesUntil(this (int hour, int minute) startTime, (int hour, int minute) endTime)
        {
            return GetMinutes(endTime) - GetMinutes(startTime);
        }

        private static int GetMinutes(this (int hour, int minute) time)
        {
            return time.hour * 60 + time.minute;
        }

        /// <summary>
        /// 接受兩組開始時間與結束時間，計算兩者間是否存在重疊，不支援跨日。
        /// </summary>
        /// <returns>分鐘數</returns>
        private static bool AreCrossingTimes(int startHourA, int startMinuteA, int endHourA, int endMinuteA
            , int startHourB, int startMinuteB, int endHourB, int endMinuteB)
        {
            return ((startHourA, startMinuteA).GetMinutes(), (endHourA, endMinuteA).GetMinutes()).IsCrossingWith(
                ((startHourB, startMinuteB).GetMinutes(), (endHourB, endMinuteB).GetMinutes()));
        }

        public static bool IsCrossingWith(this ((int hour, int minute) start, (int hour, int minute) end) timeA,
            ((int hour, int minute) start, (int hour, int minute) end) timeB)
        {
            return AreCrossingTimes(timeA.start.hour, timeA.start.minute, timeA.end.hour, timeA.end.minute,
                timeB.start.hour, timeB.start.minute, timeB.end.hour, timeB.end.minute);
        }
        
        public static bool IsIncluding(this ((int hour, int minute) start, (int hour, int minute) end) timeA,
            ((int hour, int minute) start, (int hour, int minute) end) timeB)
        {
            return ((timeA.start.hour, timeA.start.minute).GetMinutes(), (timeA.end.hour, timeA.end.minute).GetMinutes()).IsIncluding(
                ((timeB.start.hour, timeB.start.minute).GetMinutes(), (timeB.end.hour, timeB.end.minute).GetMinutes()));
        }
    }

    [Flags]
    public enum DateTimeParseType
    {
        Date = 1,
        DateTime = 2,
        YearMonth = 4
    }
}