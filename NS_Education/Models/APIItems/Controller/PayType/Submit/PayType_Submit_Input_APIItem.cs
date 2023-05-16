namespace NS_Education.Models.APIItems.Controller.PayType.Submit
{
    public class PayType_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int DPTID { get; set; }
        public int BCID { get; set; }
        
        public string Code { get; set; }
        public string Title { get; set; }
        public string AccountingNo { get; set; }
        public string CustomerNo { get; set; }
        
        public bool InvoiceFlag { get; set; }
        public bool DepositFlag { get; set; }
        public bool RestaurantFlag { get; set; }
        public bool SimpleCheckoutFlag { get; set; }
        public bool SimpleDepositFlag { get; set; }
    }
}