using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class Resver_Head
    {
        public Resver_Head()
        {
            M_Resver_TimeSpan = new HashSet<M_Resver_TimeSpan>();
            Resver_Bill = new HashSet<Resver_Bill>();
            Resver_GiveBack = new HashSet<Resver_GiveBack>();
            Resver_Other = new HashSet<Resver_Other>();
            Resver_Site = new HashSet<Resver_Site>();
        }

        public int RHID { get; set; }
        public string Code { get; set; }
        public DateTime SDate { get; set; }
        public DateTime EDate { get; set; }
        public int BSCID12 { get; set; }
        public int BSCID11 { get; set; }
        public string Title { get; set; }
        public int CID { get; set; }
        public string CustomerTitle { get; set; }
        public string ContactName { get; set; }
        public int PeopleCt { get; set; }
        public string Note { get; set; }
        public int MK_BUID { get; set; }
        public string MK_Phone { get; set; }
        public int OP_BUID { get; set; }
        public string OP_Phone { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
        public bool CheckFlag { get; set; }
        public bool CheckInFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_StaticCode BSCID11Navigation { get; set; }
        public virtual B_StaticCode BSCID12Navigation { get; set; }
        public virtual Customer C { get; set; }
        public virtual BusinessUser MK_BU { get; set; }
        public virtual BusinessUser OP_BU { get; set; }
        public virtual ICollection<M_Resver_TimeSpan> M_Resver_TimeSpan { get; set; }
        public virtual ICollection<Resver_Bill> Resver_Bill { get; set; }
        public virtual ICollection<Resver_GiveBack> Resver_GiveBack { get; set; }
        public virtual ICollection<Resver_Other> Resver_Other { get; set; }
        public virtual ICollection<Resver_Site> Resver_Site { get; set; }
    }
}
