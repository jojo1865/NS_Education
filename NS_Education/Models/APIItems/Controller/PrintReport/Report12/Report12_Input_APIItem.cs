namespace NS_Education.Models.APIItems.Controller.PrintReport.Report12
{
    /// <summary>
    /// 場地使用率一覽表的輸入物件。
    /// </summary>
    public class Report12_Input_APIItem : BaseRequestForPagedList
    {
        public int PeriodTotal { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}