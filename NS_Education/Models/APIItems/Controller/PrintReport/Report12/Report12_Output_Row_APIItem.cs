namespace NS_Education.Models.APIItems.Controller.PrintReport.Report12
{
    /// <summary>
    /// 場地使用率一覽表的單筆輸出物件。
    /// </summary>
    public class Report12_Output_Row_APIItem
    {
        public string SiteName { get; set; }
        public string SiteCode { get; set; }
        public int PeopleCt { get; set; }
        public int Days { get; set; }
        public int Periods { get; set; }
        public string Usage { get; set; }
        public int AreaSize { get; set; }
    }
}