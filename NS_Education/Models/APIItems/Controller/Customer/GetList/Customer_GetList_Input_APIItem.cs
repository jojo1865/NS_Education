namespace NS_Education.Models.APIItems.Controller.Customer.GetList
{
    public class Customer_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int BSCID6 { get; set; } = 0;
        public int BSCID4 { get; set; } = 0;
        public int BUID { get; set; } = 0;
        public int ResverType { get; set; } = -1;
        public string ContactName { get; set; }
    }
}