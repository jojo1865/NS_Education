namespace NS_Education.Models.APIItems.Resver.GetHeadList
{
    public class Resver_GetHeadList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public string TargetDate { get; set; }
        public int CID { get; set; }
        public int BSCID12 { get; set; }
    }
}