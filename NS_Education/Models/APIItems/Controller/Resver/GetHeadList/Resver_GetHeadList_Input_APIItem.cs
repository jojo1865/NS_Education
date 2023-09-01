namespace NS_Education.Models.APIItems.Controller.Resver.GetHeadList
{
    public class Resver_GetHeadList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public string TargetDate { get; set; }
        public int CID { get; set; }
        public int State { get; set; }
    }
}