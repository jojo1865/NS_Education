namespace NS_Education.Models.APIItems.Controller.PrintReport.Report6
{
    /// <summary>
    /// 人次統計表的單筆輸出物件。
    /// </summary>
    public class Report6_Output_Row_APIItem
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int RHID { get; set; }
        public string CustomerCode { get; set; }
        public string HostName { get; set; }
        public string CustomerType { get; set; }
        public string MkSales { get; set; }
        public string OpSales { get; set; }
        public string EventName { get; set; }
        public int PeopleCt { get; set; }
        public int PersonTime { get; set; }
    }
}