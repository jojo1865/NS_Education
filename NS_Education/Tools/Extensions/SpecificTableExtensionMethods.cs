using NS_Education.Models.Entities;

namespace NS_Education.Tools.Extensions
{
    public static class SpecificTableExtensionMethods
    {
        /// <summary>
        /// 依據 D_TimeSpan，取得一個內容為「HH:mm ~ HH:mm」格式的字串。<br/>
        /// 兩個時間分別代表開始時間與結束時間。
        /// </summary>
        /// <param name="dts">D_TimeSpan</param>
        /// <returns>表示開始時間與結束時間的「HH:mm ~ HH:mm」格式的字串</returns>
        public static string GetTimeRangeFormattedString(this D_TimeSpan dts)
        {
            return dts == null ? null : $"{(dts.HourS, dts.MinuteS).ToFormattedHourAndMinute()} ~ {(dts.HourE, dts.MinuteE).ToFormattedHourAndMinute()}";
        }

        /// <summary>
        /// 驗證兩個 D_TimeSpan 的時間範圍間是否存在重疊。精準至分鐘，僅起始分或結束分重疊時不算。
        /// </summary>
        /// <param name="thisDts">D_TimeSpan A</param>
        /// <param name="thatDts">D_TimeSpan B</param>
        /// <returns>
        /// true：存在重疊區間<br/>
        /// false：不存在重疊
        /// </returns>
        public static bool IsCrossingWith(this D_TimeSpan thisDts, D_TimeSpan thatDts)
        {
            if (thisDts == null || thatDts == null)
                return false;

            return ((thisDts.HourS, thisDts.MinuteS), (thisDts.HourE, thisDts.MinuteE))
                .IsCrossingWith(((thatDts.HourS, thatDts.MinuteS), (thatDts.HourE, thatDts.MinuteE)));
        }

        public static bool IsIncluding(this D_TimeSpan bigDts, D_TimeSpan smallDts)
        {
            if (bigDts == null || smallDts == null)
                return false;
            
            return ((bigDts.HourS, bigDts.MinuteS), (bigDts.HourE, bigDts.MinuteE))
                .IsIncluding(((smallDts.HourS, smallDts.MinuteS), (smallDts.HourE, smallDts.MinuteE)));
        }
    }
}