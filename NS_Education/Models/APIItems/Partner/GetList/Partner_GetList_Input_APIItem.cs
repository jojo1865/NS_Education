namespace NS_Education.Models.APIItems.Partner.GetList
{
    public class Partner_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int BCID { get; set; }
        public int BSCID { get; set; }
    }
}