namespace NS_Education.Models.APIItems.Controller.PrintReport.Distribution
{
    /// <summary>
    /// 處理匯出報表時，判定種類並分發用的 DTO。
    /// </summary>
    public class PrintReport_Distribution_Input_APIItem
    {
        /// <summary>
        /// 報表種類。
        /// </summary>
        public int ReportType { get; set; }
    }
}