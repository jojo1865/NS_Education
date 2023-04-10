using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class D_OtherPayItem
    {
        public D_OtherPayItem()
        {
            Resver_Other = new HashSet<Resver_Other>();
        }

        public int DOPIID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public int PaidType { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual ICollection<Resver_Other> Resver_Other { get; set; }
    }
}
