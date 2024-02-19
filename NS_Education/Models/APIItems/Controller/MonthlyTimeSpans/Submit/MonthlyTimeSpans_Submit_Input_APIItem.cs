namespace NS_Education.Models.APIItems.Controller.MonthlyTimeSpans.Submit
{
    /// <summary>
    /// 每月可用時段檔 Submit 的傳入物件。
    /// </summary>
    public class MonthlyTimeSpans_Submit_Input_APIItem
    {
        /// <summary>
        /// 西元年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 每月可用時段數。長度必須為 12。所有數字必須 >= 0。
        /// </summary>
        public int[] MonthlyCt { get; set; }
    }
}