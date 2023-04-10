using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class Customer
    {
        public Customer()
        {
            CustomerGift = new HashSet<CustomerGift>();
            CustomerQuestion = new HashSet<CustomerQuestion>();
            CustomerVisit = new HashSet<CustomerVisit>();
            M_Customer_Category = new HashSet<M_Customer_Category>();
        }

        public int CID { get; set; }
        public int BSCID6 { get; set; }
        public int BSCID4 { get; set; }
        public string Code { get; set; }
        public string Compilation { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public string Email { get; set; }
        public string InvoiceTitle { get; set; }
        public string ContectName { get; set; }
        public string ContectPhone { get; set; }
        public string Website { get; set; }
        public string Note { get; set; }
        public bool BillFlag { get; set; }
        public bool InFlag { get; set; }
        public bool PotentialFlag { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_StaticCode BSCID4Navigation { get; set; }
        public virtual B_StaticCode BSCID6Navigation { get; set; }
        public virtual ICollection<CustomerGift> CustomerGift { get; set; }
        public virtual ICollection<CustomerQuestion> CustomerQuestion { get; set; }
        public virtual ICollection<CustomerVisit> CustomerVisit { get; set; }
        public virtual ICollection<M_Customer_Category> M_Customer_Category { get; set; }
    }
}
