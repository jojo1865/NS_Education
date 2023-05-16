namespace NS_Education.Models.APIItems.Controller.Customer.GetList
{
    public class Customer_GetList_Input_APIItem : BaseRequestForPagedList
    {
        public string Keyword { get; set; }
        public int BSCID6 { get; set; }
        public int BSCID4 { get; set; }
        public int BUID { get; set; }
        public int ResverType { get; set; }
    }
}