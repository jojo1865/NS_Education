namespace NS_Education.Models.APIItems.Controller.PrintReport.Report14
{
    /// <summary>
    /// 場地實際銷售統計表的單筆輸出物件。
    /// </summary>
    public class Report15_Output_Row_APIItem
    {
        public string SiteType { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public string TimeSpan { get; set; }
        public int UseCount { get; set; }
        public int TotalQuotedPrice { get; set; }
        public int AverageQuotedPrice { get; set; }
        public int FixedPrice { get; set; }
    }
}