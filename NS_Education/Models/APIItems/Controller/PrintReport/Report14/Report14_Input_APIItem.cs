namespace NS_Education.Models.APIItems.Controller.PrintReport.Report13
{
    /// <summary>
    /// 場地實際使用統計表的輸入物件。
    /// </summary>
    public class Report14_Input_APIItem : BaseRequestForPagedList
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}