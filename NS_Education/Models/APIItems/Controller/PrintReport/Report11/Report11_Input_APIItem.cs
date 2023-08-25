namespace NS_Education.Models.APIItems.Controller.PrintReport.Report11
{
    /// <summary>
    /// 場地庫存狀況表的輸入物件。
    /// </summary>
    public class Report11_Input_APIItem : BaseRequestForPagedList
    {
        public string TargetDate { get; set; }
    }
}