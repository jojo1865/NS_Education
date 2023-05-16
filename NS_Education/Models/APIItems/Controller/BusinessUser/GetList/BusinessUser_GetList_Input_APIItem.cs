namespace NS_Education.Models.APIItems.Controller.BusinessUser.GetList
{
    public class BusinessUser_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int MKType { get; set; } = -1;
        public int OPType { get; set; } = -1;
    }
}