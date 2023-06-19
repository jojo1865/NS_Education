using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.CustomerGift.Submit
{
    public class CustomerGift_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public ICollection<CustomerGift_Submit_Customers_Row_APIItem> Customers { get; set; } =
            new List<CustomerGift_Submit_Customers_Row_APIItem>();

        public int GSID { get; set; }
        public int Year { get; set; }
        public string SendDate { get; set; }
        public int BSCID { get; set; }
        public string BSC_Title { get; set; }
        public string Note { get; set; }
    }
}