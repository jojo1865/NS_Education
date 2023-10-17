namespace NS_Education.Models.APIItems.Controller.PrintReport.Report16
{
    /// <summary>
    /// 場地使用一覽表的輸入物件。
    /// </summary>
    public class Report16_Input_APIItem : BaseRequestForPagedList
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string SiteName { get; set; }
        public int? BCID { get; set; }
        public bool? IsActive { get; set; }
        public int? BSCID1 { get; set; }
        public int? BasicSize { get; set; }
    }
}