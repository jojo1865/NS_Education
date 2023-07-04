using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.SiteData.GetListForCalendar
{
    public class SiteData_GetListForCalendar_TimeSpan_APIItem
    {
        public string Title { get; set; }
        public int SortNo { get; set; }

        public ICollection<SiteData_GetListForCalendar_ReservedSite_APIItem> ReservedSites { get; set; } =
            new List<SiteData_GetListForCalendar_ReservedSite_APIItem>();
    }

    public class SiteData_GetListForCalendar_ReservedSite_APIItem
    {
        public string BS_Code { get; set; }
        public string BS_Title { get; set; }
        public int RHID { get; set; }
        public string RH_Code { get; set; }
        public string RH_Title { get; set; }
        public int SortNo { get; set; }
    }
}