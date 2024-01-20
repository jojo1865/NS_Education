using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Customer.Submit
{
    public class Customer_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int CID { get; set; }
        public int BSCID6 { get; set; }
        public int BSCID4 { get; set; }
        public string Code { get; set; }
        public string Compilation { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int DZID { get; set; } = 0;
        public string Address { get; set; }
        public string Email { get; set; }
        public string InvoiceTitle { get; set; }
        public string ContactName { get; set; }

        public int ContactType1 { get; set; } = -1;
        public string ContactData1 { get; set; }
        public int ContactType2 { get; set; } = -1;
        public string ContactData2 { get; set; }
        public string Website { get; set; }
        public string Note { get; set; }
        public bool BillFlag { get; set; }
        public bool InFlag { get; set; }
        public bool PotentialFlag { get; set; }

        public ICollection<Customer_Submit_BUID_APIItem> Items { get; set; } =
            new List<Customer_Submit_BUID_APIItem>();

        public int TypeFlag { get; set; }
    }
}