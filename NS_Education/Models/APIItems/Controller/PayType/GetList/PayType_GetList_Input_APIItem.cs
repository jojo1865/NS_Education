namespace NS_Education.Models.APIItems.Controller.PayType.GetList
{
    public class PayType_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int BCID { get; set; }
    }
}