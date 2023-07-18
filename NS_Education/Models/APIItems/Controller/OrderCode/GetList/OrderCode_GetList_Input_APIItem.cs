namespace NS_Education.Models.APIItems.Controller.OrderCode.GetList
{
    public class OrderCode_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public string CodeType { get; set; }
    }
}