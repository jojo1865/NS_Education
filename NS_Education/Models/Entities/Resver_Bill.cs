using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class Resver_Bill
    {
        public int RBID { get; set; }
        public int RHID { get; set; }
        public int BCID { get; set; }
        public int DPTID { get; set; }
        public int Price { get; set; }
        public bool PayFlag { get; set; }
        public DateTime PayDate { get; set; }
        public int CheckUID { get; set; }
        public string Note { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_Category BC { get; set; }
        public virtual D_PayType DPT { get; set; }
        public virtual Resver_Head RH { get; set; }
    }
}
