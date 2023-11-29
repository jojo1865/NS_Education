using System;
using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report2
{
    public class Report2_Output_Row_Site_APIItem
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public IEnumerable<string> Lines { get; set; } = Array.Empty<string>();
        public string SeatImage { get; set; }

        public string Note { get; set; }
    }
}