using System;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report7
{
    /// <summary>
    /// 營運報表明細的單筆輸出物件。
    /// </summary>
    public class Report7_Output_Row_APIItem
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int RHID { get; set; }
        public string CustomerCode { get; set; }
        public string HostName { get; set; }
        public string FirstTradeDate { get; set; }
        public string CustomerType { get; set; }
        public string MkSales { get; set; }
        public string OpSales { get; set; }
        public string EventName { get; set; }
        public int PersonTime { get; set; }
        public int TotalProfit => FoodPrice + OtherPrice + TotalPrice;
        public int FoodPrice { get; set; }
        public int ResidencePrice { get; set; }
        public int InsurancePrice { get; set; }
        public int TransportationPrice { get; set; }
        public int OtherPrice { get; set; }
        public int TotalPrice => (int)Math.Round(TotalPriceWithoutTax * 1.05m);
        public int Deposit { get; set; }
        public int Discount { get; set; }
        public int TotalPriceWithoutTax { get; set; }
        public int AccountPrice => TotalPriceWithoutTax + Discount;
        public int SiteCapacityTotal { get; set; }

        public bool IsFirstTrade { get; set; }
    }
}