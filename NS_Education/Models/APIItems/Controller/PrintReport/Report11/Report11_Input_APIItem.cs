namespace NS_Education.Models.APIItems.Controller.PrintReport.Report11
{
    /// <summary>
    /// 場地庫存狀況表的輸入物件。
    /// </summary>
    public class Report11_Input_APIItem : BaseRequestForPagedList
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