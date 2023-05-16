using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.GetResverListByIds2
{
    public class PrintReport_GetResverListByIds2_PayItem_APIItem
    {
        public string Title { get; set; }

        public ICollection<PrintReport_GetResverListByIds2_PayItemDetail_APIItem> Items =
            new List<PrintReport_GetResverListByIds2_PayItemDetail_APIItem>();
    }
}