namespace NS_Education.Models.APIItems.Controller.SiteData.GetList
{
    public class SiteData_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int BCID { get; set; }
        public int BSCID1 { get; set; }
        public int BasicCapacity { get; set; } = 0;
        public int MaxCapacity { get; set; } = 0;
        public string TargetDate { get; set; }
        public bool? IsCombinedSiteMaster { get; set; }
    }
}