using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PayType.GetInfoById
{
    public class PayType_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public int DPTID { get; set; }
        public int BCID { get; set; }
        public string BC_TitleC { get; set; }
        public string BC_TitleE { get; set; }

        public ICollection<BaseResponseRowForSelectable> BC_List { get; set; } =
            new List<BaseResponseRowForSelectable>();

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