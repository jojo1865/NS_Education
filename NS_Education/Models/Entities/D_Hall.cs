using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class D_Hall
    {
        public D_Hall()
        {
            B_Device = new HashSet<B_Device>();
            B_PartnerItem = new HashSet<B_PartnerItem>();
            B_SiteData = new HashSet<B_SiteData>();
        }

        public int DHID { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int DDID { get; set; }
        public bool DiscountFlag { get; set; }
        public bool CheckoutNowFlag { get; set; }
        public bool PrintCheckFlag { get; set; }
        public bool Invoice3Flag { get; set; }
        public int CheckType { get; set; }
        public int BusinessTaxRatePercentage { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual D_Department DD { get; set; }
        public virtual ICollection<B_Device> B_Device { get; set; }
        public virtual ICollection<B_PartnerItem> B_PartnerItem { get; set; }
        public virtual ICollection<B_SiteData> B_SiteData { get; set; }
    }
}
