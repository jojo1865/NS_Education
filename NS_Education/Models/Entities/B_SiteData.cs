using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class B_SiteData
    {
        public B_SiteData()
        {
            M_SiteGroupGroup = new HashSet<M_SiteGroup>();
            M_SiteGroupMaster = new HashSet<M_SiteGroup>();
            Resver_Site = new HashSet<Resver_Site>();
        }

        public int BSID { get; set; }
        public int BCID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int BasicSize { get; set; }
        public int MaxSize { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public bool CubicleFlag { get; set; }
        public int BSCID1 { get; set; }
        public int BSCID5 { get; set; }
        public int DHID { get; set; }
        public int BOCID { get; set; }
        public string PhoneExt1 { get; set; }
        public string PhoneExt2 { get; set; }
        public string PhoneExt3 { get; set; }
        public string Note { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_Category BC { get; set; }
        public virtual B_OrderCode BOC { get; set; }
        public virtual B_StaticCode BSCID1Navigation { get; set; }
        public virtual B_StaticCode BSCID5Navigation { get; set; }
        public virtual D_Hall DH { get; set; }
        public virtual ICollection<M_SiteGroup> M_SiteGroupGroup { get; set; }
        public virtual ICollection<M_SiteGroup> M_SiteGroupMaster { get; set; }
        public virtual ICollection<Resver_Site> Resver_Site { get; set; }
    }
}
