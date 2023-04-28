namespace NS_Education.Models.APIItems.PartnerItem.GetList
{
    public class PartnerItem_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int BPID { get; set; }
        public int BSCID { get; set; }
        public int DHID { get; set; }
        public int BOCID { get; set; }
    }
}