using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Resver.Submit
{
    public class Resver_Submit_Input_APIItem : BaseRequestForSubmit
    {
        public int RHID { get; set; }
        public int BSCID11 { get; set; }
        public string Title { get; set; }
        public string SDate { get; set; }
        public string EDate { get; set; }
        public int PeopleCt { get; set; }
        public int CID { get; set; }
        public string CustomerTitle { get; set; }
        public string ContactName { get; set; }
        public int MK_BUID { get; set; }
        public string MK_Phone { get; set; }
        public int OP_BUID { get; set; }
        public string OP_Phone { get; set; }
        public string Note { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }

        public ICollection<Resver_Submit_ContactItem_Input_APIItem> ContactItems { get; set; } =
            new List<Resver_Submit_ContactItem_Input_APIItem>();

        public ICollection<Resver_Submit_SiteItem_Input_APIItem> SiteItems { get; set; } =
            new List<Resver_Submit_SiteItem_Input_APIItem>();

        public ICollection<Resver_Submit_OtherItem_Input_APIItem> OtherItems { get; set; } =
            new List<Resver_Submit_OtherItem_Input_APIItem>();

        public ICollection<Resver_Submit_BillItem_Input_APIItem> BillItems { get; set; } =
            new List<Resver_Submit_BillItem_Input_APIItem>();

        public ICollection<Resver_Submit_GiveBackItem_Input_APIItem> GiveBackItems { get; set; } =
            new List<Resver_Submit_GiveBackItem_Input_APIItem>();
    }
}