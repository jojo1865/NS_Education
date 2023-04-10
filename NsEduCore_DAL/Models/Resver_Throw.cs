using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class Resver_Throw
    {
        public Resver_Throw()
        {
            Resver_Throw_Food = new HashSet<Resver_Throw_Food>();
        }

        public int RTID { get; set; }
        public int RHID { get; set; }
        public DateTime TargetDate { get; set; }
        public int BSCID { get; set; }
        public int BSID { get; set; }
        public int BOCID { get; set; }
        public string Title { get; set; }
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int UnitPrice { get; set; }
        public int Pricing { get; set; }
        public int Ct { get; set; }
        public int TotalPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_SiteData BS { get; set; }
        public virtual B_StaticCode BSC { get; set; }
        public virtual Resver_Head RH { get; set; }
        public virtual ICollection<Resver_Throw_Food> Resver_Throw_Food { get; set; }
    }
}
