﻿//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace NS_Education.Models.Entities
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class db_NS_EducationEntities : DbContext
    {
        public db_NS_EducationEntities()
            : base("name=db_NS_EducationEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<B_Category> B_Category { get; set; }
        public virtual DbSet<B_Device> B_Device { get; set; }
        public virtual DbSet<B_OrderCode> B_OrderCode { get; set; }
        public virtual DbSet<B_Partner> B_Partner { get; set; }
        public virtual DbSet<B_PartnerItem> B_PartnerItem { get; set; }
        public virtual DbSet<B_SiteData> B_SiteData { get; set; }
        public virtual DbSet<B_StaticCode> B_StaticCode { get; set; }
        public virtual DbSet<BusinessUser> BusinessUser { get; set; }
        public virtual DbSet<Customer> Customer { get; set; }
        public virtual DbSet<CustomerGift> CustomerGift { get; set; }
        public virtual DbSet<CustomerQuestion> CustomerQuestion { get; set; }
        public virtual DbSet<CustomerVisit> CustomerVisit { get; set; }
        public virtual DbSet<D_Company> D_Company { get; set; }
        public virtual DbSet<D_Department> D_Department { get; set; }
        public virtual DbSet<D_FoodCategory> D_FoodCategory { get; set; }
        public virtual DbSet<D_Hall> D_Hall { get; set; }
        public virtual DbSet<D_OtherPayItem> D_OtherPayItem { get; set; }
        public virtual DbSet<D_PayType> D_PayType { get; set; }
        public virtual DbSet<D_TimeSpan> D_TimeSpan { get; set; }
        public virtual DbSet<D_Zip> D_Zip { get; set; }
        public virtual DbSet<GroupData> GroupData { get; set; }
        public virtual DbSet<M_Contect> M_Contect { get; set; }
        public virtual DbSet<M_Customer_BusinessUser> M_Customer_BusinessUser { get; set; }
        public virtual DbSet<M_Customer_Category> M_Customer_Category { get; set; }
        public virtual DbSet<M_Department_Category> M_Department_Category { get; set; }
        public virtual DbSet<M_Group_Menu> M_Group_Menu { get; set; }
        public virtual DbSet<M_Group_User> M_Group_User { get; set; }
        public virtual DbSet<M_Resver_TimeSpan> M_Resver_TimeSpan { get; set; }
        public virtual DbSet<M_SiteGroup> M_SiteGroup { get; set; }
        public virtual DbSet<MenuAPI> MenuAPI { get; set; }
        public virtual DbSet<MenuData> MenuData { get; set; }
        public virtual DbSet<Resver_Bill> Resver_Bill { get; set; }
        public virtual DbSet<Resver_Device> Resver_Device { get; set; }
        public virtual DbSet<Resver_GiveBack> Resver_GiveBack { get; set; }
        public virtual DbSet<Resver_Head> Resver_Head { get; set; }
        public virtual DbSet<Resver_Other> Resver_Other { get; set; }
        public virtual DbSet<Resver_Site> Resver_Site { get; set; }
        public virtual DbSet<Resver_Throw> Resver_Throw { get; set; }
        public virtual DbSet<Resver_Throw_Food> Resver_Throw_Food { get; set; }
        public virtual DbSet<UserData> UserData { get; set; }
        public virtual DbSet<UserLog> UserLog { get; set; }
        public virtual DbSet<UserPasswordLog> UserPasswordLog { get; set; }
    }
}
