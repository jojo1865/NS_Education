namespace NS_Education.Models.APIItems.Controller.PrintReport.Report5
{
    /// <summary>
    /// 餐飲明細表的單筆輸出物件。
    /// </summary>
    public class Report5_Output_Row_APIItem
    {
        public string ReserveDate { get; set; }
        public int RHID { get; set; }
        public string EventName { get; set; }
        public string PartnerName { get; set; }
        public string CuisineType { get; set; }
        public string CuisineName { get; set; }
        public string HostName { get; set; }
        public int ReservedQuantity { get; set; }
        public int UnitPrice { get; set; }
        public int UnitPriceSum => UnitPrice * ReservedQuantity;
        public int QuotedPrice { get; set; }
        public int QuotedPriceSum => QuotedPrice * ReservedQuantity;
    }
}