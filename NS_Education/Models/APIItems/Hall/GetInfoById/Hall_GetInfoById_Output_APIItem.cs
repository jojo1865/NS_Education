using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Hall.GetInfoById
{
    public class Hall_GetInfoById_Output_APIItem : BaseGetResponseInfusableWithCreUpd
    {
        public int DHID { get; set; }
        public int DDID { get; set; }
        public string DD_TitleC { get; set; }
        public string DD_TitleE { get; set; }

        public ICollection<BaseResponseRowForSelectable> DD_List { get; set; } =
            new List<BaseResponseRowForSelectable>();
        
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public bool DiscountFlag { get; set; }
        public bool CheckoutNowFlag { get; set; }
        public bool PrintCheckFlag { get; set; }
        public bool Invoice3Flag { get; set; }
        public int CheckType { get; set; }
        public int BusinessTaxRatePercentage { get; set; }
        public int DeviceCt { get; set; }
        public int SiteCt { get; set; }
        public int PartnerItemCt { get; set; }
    }
}