namespace NS_Education.Models.APIItems.Controller.Customer.Submit
{
    public class Customer_Submit_BUID_APIItem
    {
        public int BUID { get; set; }
        public int ContactType { get; set; } = -1;
        public string ContactData { get; set; }
        public bool MKSalesFlag { get; set; }
        public bool OPSalesFlag { get; set; }
    }
}