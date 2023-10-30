namespace NS_Education.Models.APIItems.Controller.PrintReport.Report8
{
    /// <summary>
    /// 滿意度調查表報表的輸入物件。
    /// </summary>
    public class Report8_Input_APIItem : BaseRequestForPagedList
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}