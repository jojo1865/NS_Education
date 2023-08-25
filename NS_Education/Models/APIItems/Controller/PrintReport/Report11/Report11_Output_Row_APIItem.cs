using System;
using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report11
{
    /// <summary>
    /// 場地庫存狀況表的單筆輸出物件。
    /// </summary>
    public class Report11_Output_Row_APIItem
    {
        public string Name { get; set; }

        public IEnumerable<Report11_Output_Row_Site_APIItem> Sites { get; set; } =
            Array.Empty<Report11_Output_Row_Site_APIItem>();
    }
}