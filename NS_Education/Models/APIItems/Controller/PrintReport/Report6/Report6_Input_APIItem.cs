namespace NS_Education.Models.APIItems.Controller.PrintReport.Report6
{
    /// <summary>
    /// 人次統計表的輸入物件。
    /// </summary>
    public class Report6_Input_APIItem : BaseRequestForPagedList
    {
        public bool Internal { get; set; } = false;
        public bool External { get; set; } = false;
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}