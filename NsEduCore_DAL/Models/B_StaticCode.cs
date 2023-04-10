using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class B_StaticCode
    {
        public B_StaticCode()
        {
            B_Device = new HashSet<B_Device>();
            B_Partner = new HashSet<B_Partner>();
            B_PartnerItem = new HashSet<B_PartnerItem>();
            B_SiteDataBSCID1Navigation = new HashSet<B_SiteData>();
            B_SiteDataBSCID5Navigation = new HashSet<B_SiteData>();
            CustomerBSCID4Navigation = new HashSet<Customer>();
            CustomerBSCID6Navigation = new HashSet<Customer>();
            CustomerVisit = new HashSet<CustomerVisit>();
            Resver_HeadBSCID11Navigation = new HashSet<Resver_Head>();
            Resver_HeadBSCID12Navigation = new HashSet<Resver_Head>();
            Resver_Site = new HashSet<Resver_Site>();
            Resver_Throw = new HashSet<Resver_Throw>();
            Resver_Throw_Food = new HashSet<Resver_Throw_Food>();
        }

        public int BSCID { get; set; }
        public int CodeType { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual ICollection<B_Device> B_Device { get; set; }
        public virtual ICollection<B_Partner> B_Partner { get; set; }
        public virtual ICollection<B_PartnerItem> B_PartnerItem { get; set; }
        public virtual ICollection<B_SiteData> B_SiteDataBSCID1Navigation { get; set; }
        public virtual ICollection<B_SiteData> B_SiteDataBSCID5Navigation { get; set; }
        public virtual ICollection<Customer> CustomerBSCID4Navigation { get; set; }
        public virtual ICollection<Customer> CustomerBSCID6Navigation { get; set; }
        public virtual ICollection<CustomerVisit> CustomerVisit { get; set; }
        public virtual ICollection<Resver_Head> Resver_HeadBSCID11Navigation { get; set; }
        public virtual ICollection<Resver_Head> Resver_HeadBSCID12Navigation { get; set; }
        public virtual ICollection<Resver_Site> Resver_Site { get; set; }
        public virtual ICollection<Resver_Throw> Resver_Throw { get; set; }
        public virtual ICollection<Resver_Throw_Food> Resver_Throw_Food { get; set; }
    }
}
