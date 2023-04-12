﻿using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class B_OrderCode
    {
        public B_OrderCode()
        {
            B_Device = new HashSet<B_Device>();
            B_PartnerItem = new HashSet<B_PartnerItem>();
            B_SiteData = new HashSet<B_SiteData>();
            D_FoodCategory = new HashSet<D_FoodCategory>();
            Resver_Device = new HashSet<Resver_Device>();
            Resver_Other = new HashSet<Resver_Other>();
            Resver_Site = new HashSet<Resver_Site>();
            Resver_Throw = new HashSet<Resver_Throw>();
        }

        public int BOCID { get; set; }
        public int CodeType { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int SortNo { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual ICollection<B_Device> B_Device { get; set; }
        public virtual ICollection<B_PartnerItem> B_PartnerItem { get; set; }
        public virtual ICollection<B_SiteData> B_SiteData { get; set; }
        public virtual ICollection<D_FoodCategory> D_FoodCategory { get; set; }
        public virtual ICollection<Resver_Device> Resver_Device { get; set; }
        public virtual ICollection<Resver_Other> Resver_Other { get; set; }
        public virtual ICollection<Resver_Site> Resver_Site { get; set; }
        public virtual ICollection<Resver_Throw> Resver_Throw { get; set; }
    }
}
