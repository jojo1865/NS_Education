namespace NS_Education.Models.APIItems.Controller.PrintReport.Report10
{
    /// <summary>
    /// 未成交原因分析的輸入物件。
    /// </summary>
    public class Report10_Input_APIItem : BaseRequestForPagedList
    {
        public string TargetDate { get; set; }
    }
}