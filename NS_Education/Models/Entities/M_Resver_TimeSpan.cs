using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class M_Resver_TimeSpan
    {
        public int MID { get; set; }
        public int RHID { get; set; }
        public string TargetTable { get; set; }
        public int TargetID { get; set; }
        public int DTSID { get; set; }
        public int SortNo { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual D_TimeSpan DTS { get; set; }
        public virtual Resver_Head RH { get; set; }
    }
}
