using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report8
{
    /// <summary>
    /// 滿意度調查表報表的單筆輸出物件。
    /// </summary>
    public class Report8_Output_Row_APIItem
    {
        public string SiteName { get; set; }
        public string SiteCode { get; set; }
        public int RentCt { get; set; }
        public IDictionary<int, int> SiteSatisfied { get; set; }
        public IDictionary<int, int> DeviceSatisfied { get; set; }
        public IDictionary<int, int> CleanSatisfied { get; set; }
        public IDictionary<int, int> NegotiatorSatisfied { get; set; }
        public IDictionary<int, int> ServiceSatisfied { get; set; }
        public IDictionary<int, int> MealSatisfied { get; set; }
        public IDictionary<int, int> DessertSatisfied { get; set; }
        public string WillUseAgainPercentage { get; set; }
    }
}