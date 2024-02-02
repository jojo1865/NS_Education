namespace NS_Education.Models.APIItems.Controller.PrintReport.Report9
{
    /// <summary>
    /// 客戶歷史資料報表的單筆輸出物件。
    /// </summary>
    public class Report9_Output_Row_APIItem
    {
        public int RHID { get; set; }
        public string HostName { get; set; }
        public string EventName { get; set; }
        public int TotalIncome { get; set; }
        public string Date { get; set; }
        public string SiteName { get; set; }
        public string EarliestTimeSpan { get; set; }
        public string LatestTimeSpan { get; set; }
        public int SitePrice { get; set; }

        public string ContactName { get; set; }

        public string ContactContent1 { get; set; }
        public string ContactContent2 { get; set; }

        public string Email { get; set; }
    }
}