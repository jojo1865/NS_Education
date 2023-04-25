using System.Collections.Generic;

namespace NS_Education.Models.APIItems.BusinessUser.Submit
{
    public class BusinessUser_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int BUID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public bool MKsalesFlag { get; set; }
        public bool OPsalesFlag { get; set; }

        public ICollection<BusinessUser_Customer_APIItem> Items { get; set; } =
            new List<BusinessUser_Customer_APIItem>();
    }
}