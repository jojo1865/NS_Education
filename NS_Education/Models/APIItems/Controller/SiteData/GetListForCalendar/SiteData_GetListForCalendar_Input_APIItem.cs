using System;

namespace NS_Education.Models.APIItems.Controller.SiteData.GetListForCalendar
{
    public class SiteData_GetListForCalendar_Input_APIItem : BaseRequestForPagedList
    {
        public int TargetYear { get; set; } = DateTime.Now.Year;
        public int TargetMonth { get; set; } = DateTime.Now.Month;
        public int RHID { get; set; }
        public int BSID { get; set; }
        public string CustomerTitleC { get; set; }
    }
}