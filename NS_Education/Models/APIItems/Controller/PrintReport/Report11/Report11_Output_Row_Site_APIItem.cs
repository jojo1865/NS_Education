using System;
using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report11
{
    public class Report11_Output_Row_Site_APIItem
    {
        public string Name { get; set; }

        public IEnumerable<Report11_Output_Row_TimeSpan_APIItem> TimeSpans { get; set; } =
            Array.Empty<Report11_Output_Row_TimeSpan_APIItem>();
    }
}