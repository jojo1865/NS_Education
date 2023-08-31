using System;
using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report11
{
    public class Report11_Output_Row_APIItem
    {
        public string Type { get; set; }
        public string SiteName { get; set; }
        public string Time { get; set; }
        public IDictionary<DateTime, string> DateToCustomer { get; set; } = new Dictionary<DateTime, string>();
    }
}