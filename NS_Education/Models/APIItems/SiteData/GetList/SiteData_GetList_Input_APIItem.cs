namespace NS_Education.Models.APIItems.SiteData.GetList
{
    public class SiteData_GetList_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int BCID { get; set; }
        public bool? ActiveFlag { get; set; }
    }
}