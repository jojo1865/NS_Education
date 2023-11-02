namespace NS_Education.Models.APIItems.Controller.PrintReport.Report10
{
    /// <summary>
    /// 未成交原因分析的輸入物件。
    /// </summary>
    public class Report10_Input_APIItem : BaseRequestForPagedList
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int? CID { get; set; }
        public int? BSCID { get; set; }
    }
}