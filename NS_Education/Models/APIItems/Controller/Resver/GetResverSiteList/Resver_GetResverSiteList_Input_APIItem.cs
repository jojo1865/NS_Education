namespace NS_Education.Models.APIItems.Controller.Resver.GetResverSiteList
{
    public class Resver_GetResverSiteList_Input_APIItem : BaseRequestForPagedList
    {
        public string FreeDate { get; set; }
        public int BSCID1 { get; set; }
        public int PeopleCt { get; set; }
    }
}