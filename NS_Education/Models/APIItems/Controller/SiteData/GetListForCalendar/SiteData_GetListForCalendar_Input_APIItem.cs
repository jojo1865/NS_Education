namespace NS_Education.Models.APIItems.Controller.SiteData.GetListForCalendar
{
    public class SiteData_GetListForCalendar_Input_APIItem : BaseRequestForPagedList
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public int RHID { get; set; }
        public int BSID { get; set; }
        public string CustomerTitleC { get; set; }
        public string SiteTitle { get; set; }
    }
}