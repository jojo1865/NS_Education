namespace NS_Education.Models.APIItems.Controller.PrintReport.Report7
{
    /// <summary>
    /// 營運報表明細的輸入物件。
    /// </summary>
    public class Report7_Input_APIItem : BaseRequestForPagedList
    {
        public string TargetDate { get; set; }
    }
}