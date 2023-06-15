namespace NS_Education.Models.APIItems.Controller.Customer.GetUniqueNames
{
    public class Customer_GetUniqueNames_Input_APIItem : BaseRequestForList
    {
        public string Keyword { get; set; }
        public int MaxRow { get; set; } = 10;
    }
}