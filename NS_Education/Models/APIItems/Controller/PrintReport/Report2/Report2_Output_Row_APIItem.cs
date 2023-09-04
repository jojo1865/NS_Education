using System;
using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report2
{
    /// <summary>
    /// Function Order 的輸出物件。
    /// </summary>
    public class Report2_Output_Row_APIItem
    {
        public string HostName { get; set; }
        public string EventTitle { get; set; }
        public int RHID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public IEnumerable<string> SiteNames { get; set; }
        public string MKT { get; set; }
        public string Owner { get; set; }
        public string ParkingNote { get; set; }
        public string Contact { get; set; }
        public string[] PayStatus { get; set; }

        public IEnumerable<Report2_Output_Row_Site_APIItem> Sites { get; set; } =
            Array.Empty<Report2_Output_Row_Site_APIItem>();

        public IEnumerable<Report2_Output_Row_Food_APIItem> Foods { get; set; } =
            Array.Empty<Report2_Output_Row_Food_APIItem>();
    }
}