using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.GetResverListByIds1
{
    public class PrintReport_GetResverListByIds1_SiteItem_APIItem
    {
        public ICollection<PrintReport_GetResverListByIds1_DeviceItem_APIItem> DeviceItems =
            new List<PrintReport_GetResverListByIds1_DeviceItem_APIItem>();

        public ICollection<PrintReport_GetResverListByIds1_TimeSpanItem_APIItem> TimeSpanItems =
            new List<PrintReport_GetResverListByIds1_TimeSpanItem_APIItem>();

        public int RSID { get; set; }
        public string Date { get; set; }

        /// <summary>
        /// 時段範圍
        /// </summary>
        public string TimeSpans { get; set; }

        public string SiteTitle { get; set; }
        public string TableTitle { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
    }
}