namespace NS_Education.Models.APIItems.Controller.SiteData.GetList
{
    public class SiteData_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int BCID { get; set; }
    }
}