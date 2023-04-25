using System.Collections.Generic;
using NS_Education.Models.Entities;

namespace NS_Education.Models.APIItems.SiteData.GetListForCalendar
{
    public class SiteData_GetListForCalendar_Output_Row_APIItem : BaseResponseWithCreUpd<Resver_Site>
    {
        public int BSID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int RHID { get; set; }
        public int RSID { get; set; }
        public int RSSortNo { get; set; }
        public string RHCode { get; set; }
        public string RHTitle { get; set; }
        public int CID { get; set; }
        public string CustomerTitle { get; set; }
        public string TargetDate { get; set; }

        public IEnumerable<SiteData_GetListForCalendar_TimeSpan_APIItem> Items { get; set; } =
            new List<SiteData_GetListForCalendar_TimeSpan_APIItem>();
    }
}