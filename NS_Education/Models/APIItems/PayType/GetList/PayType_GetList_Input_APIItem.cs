namespace NS_Education.Models.APIItems.PayType.GetList
{
    public class PayType_GetList_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int BCID { get; set; }
    }
}