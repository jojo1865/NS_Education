using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class B_Category
    {
        public B_Category()
        {
            B_Device = new HashSet<B_Device>();
            B_Partner = new HashSet<B_Partner>();
            B_SiteData = new HashSet<B_SiteData>();
            D_Company = new HashSet<D_Company>();
            D_PayType = new HashSet<D_PayType>();
            M_Customer_Category = new HashSet<M_Customer_Category>();
            M_Department_Category = new HashSet<M_Department_Category>();
            M_Partner_Category = new HashSet<M_Partner_Category>();
            Resver_Bill_Header = new HashSet<Resver_Bill_Header>();
        }

        public int BCID { get; set; }
        public int CategoryType { get; set; }
        public int ParentID { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int SortNo { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual ICollection<B_Device> B_Device { get; set; }
        public virtual ICollection<B_Partner> B_Partner { get; set; }
        public virtual ICollection<B_SiteData> B_SiteData { get; set; }
        public virtual ICollection<D_Company> D_Company { get; set; }
        public virtual ICollection<D_PayType> D_PayType { get; set; }
        public virtual ICollection<M_Customer_Category> M_Customer_Category { get; set; }
        public virtual ICollection<M_Department_Category> M_Department_Category { get; set; }
        public virtual ICollection<M_Partner_Category> M_Partner_Category { get; set; }
        public virtual ICollection<Resver_Bill_Header> Resver_Bill_Header { get; set; }
    }
}
