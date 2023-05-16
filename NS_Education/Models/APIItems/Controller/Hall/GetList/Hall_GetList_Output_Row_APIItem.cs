namespace NS_Education.Models.APIItems.Controller.Hall.GetList
{
    public class Hall_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreUpd
    {
        public int DDID { get; set; }
        public int DHID { get; set; }
        public string DD_TitleC { get; set; }
        public string DD_TitleE { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public bool DiscountFlag { get; set; }
        public bool CheckoutNowFlag { get; set; }
        public bool PrintCheckFlag { get; set; }
        public bool Invoice3Flag { get; set; }
        public int CheckType { get; set; }
        public decimal BusinessTaxRate { get; set; }
        public int DeviceCt { get; set; }
        public int SiteCt { get; set; }
        public int PartnerItemCt { get; set; }
    }
}