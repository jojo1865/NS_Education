using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class B_Partner
    {
        public B_Partner()
        {
            B_PartnerItem = new HashSet<B_PartnerItem>();
            M_Partner_Category = new HashSet<M_Partner_Category>();
            Resver_Throw_Food = new HashSet<Resver_Throw_Food>();
        }

        public int BPID { get; set; }
        public int BCID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Compilation { get; set; }
        public int BSCID { get; set; }
        public string Email { get; set; }
        public bool CleanFlag { get; set; }
        public int CleanPrice { get; set; }
        public DateTime CleanSDate { get; set; }
        public DateTime CleanEDate { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_Category BC { get; set; }
        public virtual B_StaticCode BSC { get; set; }
        public virtual ICollection<B_PartnerItem> B_PartnerItem { get; set; }
        public virtual ICollection<M_Partner_Category> M_Partner_Category { get; set; }
        public virtual ICollection<Resver_Throw_Food> Resver_Throw_Food { get; set; }
    }
}
