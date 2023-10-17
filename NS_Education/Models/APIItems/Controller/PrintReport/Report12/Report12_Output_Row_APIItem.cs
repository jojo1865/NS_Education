namespace NS_Education.Models.APIItems.Controller.PrintReport.Report12
{
    /// <summary>
    /// 場地使用率分析表的單筆輸出物件。
    /// </summary>
    public class Report12_Output_Row_APIItem
    {
        public string PeopleCt { get; set; }
        public string SiteName { get; set; }
        public string SiteCode { get; set; }
        public int AreaSize { get; set; }
        public Report12_Output_Row_MonthlyUsage_APIItem AllUsage { get; set; }
        public Report12_Output_Row_MonthlyUsage_APIItem InternalUsage { get; set; }
        public Report12_Output_Row_MonthlyUsage_APIItem ExternalUsage { get; set; }
    }
}