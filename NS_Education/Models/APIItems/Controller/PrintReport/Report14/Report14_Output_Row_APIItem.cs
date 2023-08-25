namespace NS_Education.Models.APIItems.Controller.PrintReport.Report14
{
    /// <summary>
    /// 場地預估銷售月報表的單筆輸出物件。
    /// </summary>
    public class Report14_Output_Row_APIItem
    {
        public string SiteType { get; set; }
        public string SiteCode { get; set; }
        public string SiteName { get; set; }
        public string TimeSpan { get; set; }
        public int UseCount { get; set; }
        public string UseRate { get; set; }
    }
}