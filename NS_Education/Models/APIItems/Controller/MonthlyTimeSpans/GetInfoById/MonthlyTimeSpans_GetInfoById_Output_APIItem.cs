namespace NS_Education.Models.APIItems.Controller.MonthlyTimeSpans.GetInfoById
{
    /// <summary>
    /// 每月可用時段檔 GetInfoById 的傳入物件。
    /// </summary>
    public class MonthlyTimeSpans_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 各月可用時段數。長度必為 12。
        /// </summary>
        public int[] MonthlyCt { get; set; } = new int[12];
    }
}