using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class Resver_Head
    {
        public Resver_Head()
        {
            Resver_Bill_Header = new HashSet<Resver_Bill_Header>();
            Resver_Device = new HashSet<Resver_Device>();
            Resver_GiveBack = new HashSet<Resver_GiveBack>();
            Resver_Other = new HashSet<Resver_Other>();
            Resver_Site = new HashSet<Resver_Site>();
            Resver_Throw = new HashSet<Resver_Throw>();
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
        public bool CheckFlag { get; set; }
        public bool CheckInFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_StaticCode BSCID11Navigation { get; set; }
        public virtual B_StaticCode BSCID12Navigation { get; set; }
        public virtual ICollection<Resver_Bill_Header> Resver_Bill_Header { get; set; }
        public virtual ICollection<Resver_Device> Resver_Device { get; set; }
        public virtual ICollection<Resver_GiveBack> Resver_GiveBack { get; set; }
        public virtual ICollection<Resver_Other> Resver_Other { get; set; }
        public virtual ICollection<Resver_Site> Resver_Site { get; set; }
        public virtual ICollection<Resver_Throw> Resver_Throw { get; set; }
    }
}
