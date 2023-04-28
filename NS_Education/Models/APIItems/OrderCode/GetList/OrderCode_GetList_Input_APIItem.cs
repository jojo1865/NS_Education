namespace NS_Education.Models.APIItems.OrderCode.GetList
{
    public class OrderCode_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int CodeType { get; set; }
    }
}