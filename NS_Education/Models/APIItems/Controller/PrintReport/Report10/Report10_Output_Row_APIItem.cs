namespace NS_Education.Models.APIItems.Controller.PrintReport.Report10
{
    /// <summary>
    /// 未成交原因分析的單筆輸出物件。
    /// </summary>
    public class Report10_Output_Row_APIItem
    {
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string TargetTitle { get; set; }
        public string VisitMethod { get; set; }
        public string VisitDate { get; set; }
        public string Agent { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AfterNote { get; set; }
        public string NoDealReason { get; set; }
    }
}