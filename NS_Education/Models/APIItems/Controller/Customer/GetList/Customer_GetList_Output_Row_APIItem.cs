using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Customer.GetList
{
    public class Customer_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreUpd
    {
        public int CID { get; set; }
        
        public int BSCID6 { get; set; }
        public string BSC6_Title { get; set; }
        
        public int BSCID4 { get; set; }
        public string BSC4_Title { get; set; }
        
        public string Code { get; set; }
        public string Compilation { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public string Email { get; set; }
        public string InvoiceTitle { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string Website { get; set; }
        public string Note { get; set; }
        public bool BillFlag { get; set; }
        public bool InFlag { get; set; }
        public bool PotentialFlag { get; set; }
        public int ResverCt { get; set; }
        public int VisitCt { get; set; }
        public int QuestionCt { get; set; }
        public int GiftCt { get; set; }

        public ICollection<Customer_GetList_BusinessUser_APIItem> Items { get; set; } =
            new List<Customer_GetList_BusinessUser_APIItem>();
    }
}