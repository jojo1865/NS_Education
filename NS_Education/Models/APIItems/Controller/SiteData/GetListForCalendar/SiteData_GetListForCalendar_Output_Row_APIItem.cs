using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.SiteData.GetListForCalendar
{
    public class SiteData_GetListForCalendar_Output_Row_APIItem : BaseGetResponseRowWithCreUpd
    {
        public string Date { get; set; }
        public int Weekday { get; set; }

        public ICollection<SiteData_GetListForCalendar_TimeSpan_APIItem> TimeSpans { get; set; } =
            new List<SiteData_GetListForCalendar_TimeSpan_APIItem>();
    }
}