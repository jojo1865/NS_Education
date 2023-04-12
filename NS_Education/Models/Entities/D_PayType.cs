using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class D_PayType
    {
        public D_PayType()
        {
            Resver_Bill_Header = new HashSet<Resver_Bill_Header>();
        }

        public int DPTID { get; set; }
        public int BCID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string AccountingNo { get; set; }
        public string CustormerNo { get; set; }
        public bool InvoiceFlag { get; set; }
        public bool DepositFlag { get; set; }
        public bool RestaurantFlag { get; set; }
        public bool SimpleCheckoutFlag { get; set; }
        public bool SimpleDepositFlag { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_Category BC { get; set; }
        public virtual ICollection<Resver_Bill_Header> Resver_Bill_Header { get; set; }
    }
}
