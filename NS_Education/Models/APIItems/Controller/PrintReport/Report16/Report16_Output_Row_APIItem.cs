namespace NS_Education.Models.APIItems.Controller.PrintReport.Report16
{
    /// <summary>
    /// 場地使用一覽表的單筆輸出物件。
    /// </summary>
    public class Report16_Output_Row_APIItem
    {
        public string Date { get; set; }
        public string Site { get; set; }
        public string TimeSpan { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int RHID { get; set; }
        public string CustomerCode { get; set; }
        public string Host { get; set; }
        public string HostType { get; set; }
        public string MKSales { get; set; }
        public string OPSales { get; set; }
        public string EventName { get; set; }
        public int UnitPrice { get; set; }
        public int QuotedPrice { get; set; }
    }
}