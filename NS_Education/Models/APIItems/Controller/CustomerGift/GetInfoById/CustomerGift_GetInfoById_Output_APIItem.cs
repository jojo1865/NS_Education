using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.CustomerGift.GetInfoById
{
    public class CustomerGift_GetInfoById_Output_APIItem : BaseGetResponseRowInfusableWithCreUpd
    {
        public ICollection<CustomerGift_GetInfoById_Customers_Row_APIItem> Customers =
            new List<CustomerGift_GetInfoById_Customers_Row_APIItem>();

        public int GSID { get; set; }

        public int Year { get; set; }
        public string SendDate { get; set; }

        public int BSCID { get; set; }
        public string BSC_Code { get; set; }
        public string BSC_Title { get; set; }

        public ICollection<CommonResponseRowForSelectable> BSC_List { get; set; } =
            new List<CommonResponseRowForSelectable>();

        public string Title { get; set; }
        public string Note { get; set; }
    }
}