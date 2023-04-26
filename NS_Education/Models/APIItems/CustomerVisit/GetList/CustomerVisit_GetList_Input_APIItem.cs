namespace NS_Education.Models.APIItems.CustomerVisit.GetList
{
    public class CustomerVisit_GetList_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int CID { get; set; }
        public int BUID { get; set; }
        public string SDate { get; set; }
        public string EDate { get; set; }
        public int BSCID { get; set; }
    }
}