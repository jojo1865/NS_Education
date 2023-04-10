using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class Resver_Bill_Header
    {
        public Resver_Bill_Header()
        {
            Resver_Bill_Detail = new HashSet<Resver_Bill_Detail>();
        }

        public int RBHID { get; set; }
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
        public virtual ICollection<Resver_Bill_Detail> Resver_Bill_Detail { get; set; }
    }
}
