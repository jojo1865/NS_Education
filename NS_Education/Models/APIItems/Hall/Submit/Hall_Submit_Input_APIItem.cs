namespace NS_Education.Models.APIItems.Hall.Submit
{
    public class Hall_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int DHID { get; set; }
        public int DDID { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public bool DiscountFlag { get; set; }
        public bool CheckoutNowFlag { get; set; }
        public bool PrintCheckFlag { get; set; }
        public bool Invoice3Flag { get; set; }
        public int CheckType { get; set; }
        public double BusinessTaxRate { get; set; }
    }
}