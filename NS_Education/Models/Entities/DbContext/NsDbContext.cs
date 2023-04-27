using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NS_Education.Tools.Extensions;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities.DbContext
{
    public partial class NsDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public NsDbContext()
        {
        }

        public NsDbContext(DbContextOptions<NsDbContext> options)
            : base(options)
        {
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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured) return;
            
            var connectionStrings = System.Web.Configuration.WebConfigurationManager.ConnectionStrings;

            string env =
                Environment.ExpandEnvironmentVariables(connectionStrings["db_NS_EducationConnectionStringEnv"].ConnectionString);
            string fallback =
                connectionStrings["db_NS_EducationConnectionString"].ConnectionString;

            optionsBuilder.UseSqlServer(!env.IsNullOrWhiteSpace() ? env : fallback);
        } 
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<B_Category>(entity =>
            {
                entity.HasKey(e => e.BCID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.TitleC).HasMaxLength(50);

                entity.Property(e => e.TitleE).HasMaxLength(50);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<B_Device>(entity =>
            {
                entity.HasKey(e => e.BDID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.SupplierName).HasMaxLength(100);

                entity.Property(e => e.SupplierPhone).HasMaxLength(30);

                entity.Property(e => e.SupplierTitle).HasMaxLength(100);

                entity.Property(e => e.Title).HasMaxLength(60);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BC)
                    .WithMany(p => p.B_Device)
                    .HasForeignKey(d => d.BCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_Device_B_Category");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BOC)
                    .WithMany(p => p.B_Device)
                    .HasForeignKey(d => d.BOCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_Device_B_OrderCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSC)
                    .WithMany(p => p.B_Device)
                    .HasForeignKey(d => d.BSCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_Device_B_StaticCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.DH)
                    .WithMany(p => p.B_Device)
                    .HasForeignKey(d => d.DHID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_Device_D_Hall");
            });

            modelBuilder.Entity<B_OrderCode>(entity =>
            {
                entity.HasKey(e => e.BOCID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.PrintNote).HasMaxLength(100);

                entity.Property(e => e.PrintTitle).HasMaxLength(100);

                entity.Property(e => e.Title).HasMaxLength(60);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<B_Partner>(entity =>
            {
                entity.HasKey(e => e.BPID);

                entity.Property(e => e.CleanEDate).HasColumnType("datetime");

                entity.Property(e => e.CleanSDate).HasColumnType("datetime");

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Compilation)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.Note).HasComment("備註");

                entity.Property(e => e.Title).HasMaxLength(60);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BC)
                    .WithMany(p => p.B_Partner)
                    .HasForeignKey(d => d.BCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_Partner_B_Category");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSC)
                    .WithMany(p => p.B_Partner)
                    .HasForeignKey(d => d.BSCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_Partner_B_StaticCode");
            });

            modelBuilder.Entity<B_PartnerItem>(entity =>
            {
                entity.HasKey(e => e.BPIID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BOC)
                    .WithMany(p => p.B_PartnerItem)
                    .HasForeignKey(d => d.BOCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_PartnerItem_B_OrderCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BP)
                    .WithMany(p => p.B_PartnerItem)
                    .HasForeignKey(d => d.BPID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_PartnerItem_B_Partner");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSC)
                    .WithMany(p => p.B_PartnerItem)
                    .HasForeignKey(d => d.BSCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_PartnerItem_B_StaticCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.DH)
                    .WithMany(p => p.B_PartnerItem)
                    .HasForeignKey(d => d.DHID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_PartnerItem_D_Hall");
            });

            modelBuilder.Entity<B_SiteData>(entity =>
            {
                entity.HasKey(e => e.BSID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.PhoneExt1)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneExt2)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.PhoneExt3)
                    .HasMaxLength(6)
                    .IsUnicode(false);

                entity.Property(e => e.Title).HasMaxLength(60);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BC)
                    .WithMany(p => p.B_SiteData)
                    .HasForeignKey(d => d.BCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_SiteData_B_Category");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BOC)
                    .WithMany(p => p.B_SiteData)
                    .HasForeignKey(d => d.BOCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_SiteData_B_OrderCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSCID1Navigation)
                    .WithMany(p => p.B_SiteDataBSCID1Navigation)
                    .HasForeignKey(d => d.BSCID1)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_SiteData_B_StaticCode1");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSCID5Navigation)
                    .WithMany(p => p.B_SiteDataBSCID5Navigation)
                    .HasForeignKey(d => d.BSCID5)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_SiteData_B_StaticCode5");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.DH)
                    .WithMany(p => p.B_SiteData)
                    .HasForeignKey(d => d.DHID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_B_SiteData_D_Hall");
            });

            modelBuilder.Entity<B_StaticCode>(entity =>
            {
                entity.HasKey(e => e.BSCID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<BusinessUser>(entity =>
            {
                entity.HasKey(e => e.BUID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(30);

                entity.Property(e => e.Phone).HasMaxLength(50);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Compilation)
                    .HasMaxLength(8)
                    .IsUnicode(false);

                entity.Property(e => e.ContectName).HasMaxLength(50);

                entity.Property(e => e.ContectPhone).HasMaxLength(50);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.Email).HasMaxLength(100);

                entity.Property(e => e.InvoiceTitle).HasMaxLength(50);

                entity.Property(e => e.TitleC).HasMaxLength(50);

                entity.Property(e => e.TitleE).HasMaxLength(100);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSCID4Navigation)
                    .WithMany(p => p.CustomerBSCID4Navigation)
                    .HasForeignKey(d => d.BSCID4)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Customer_B_StaticCode4");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSCID6Navigation)
                    .WithMany(p => p.CustomerBSCID6Navigation)
                    .HasForeignKey(d => d.BSCID6)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Customer_B_StaticCode6");
            });

            modelBuilder.Entity<CustomerGift>(entity =>
            {
                entity.HasKey(e => e.CGID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.SendDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSC)
                    .WithMany(p => p.CustomerGift)
                    .HasForeignKey(d => d.BSCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "CustomerGift_B_StaticCode_BSCID_fk");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.C)
                    .WithMany(p => p.CustomerGift)
                    .HasForeignKey(d => d.CID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_CustomerGift_Customer");
            });

            modelBuilder.Entity<CustomerQuestion>(entity =>
            {
                entity.HasKey(e => e.CQID);

                entity.Property(e => e.AskArea).HasMaxLength(100);

                entity.Property(e => e.AskDate).HasColumnType("datetime");

                entity.Property(e => e.AskTitle).HasMaxLength(100);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.ResponseDate).HasColumnType("datetime");

                entity.Property(e => e.ResponseUser).HasMaxLength(100);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.C)
                    .WithMany(p => p.CustomerQuestion)
                    .HasForeignKey(d => d.CID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_CustomerQuestion_Customer");
            });

            modelBuilder.Entity<CustomerVisit>(entity =>
            {
                entity.HasKey(e => e.CVID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.TargetTitle).HasMaxLength(100);

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                entity.Property(e => e.VisitDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSC)
                    .WithMany(p => p.CustomerVisit)
                    .HasForeignKey(d => d.BSCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_CustomerVisit_B_StaticCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BU)
                    .WithMany(p => p.CustomerVisit)
                    .HasForeignKey(d => d.BUID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_CustomerVisit_BusinessUser");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.C)
                    .WithMany(p => p.CustomerVisit)
                    .HasForeignKey(d => d.CID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_CustomerVisit_Customer");
            });

            modelBuilder.Entity<D_Company>(entity =>
            {
                entity.HasKey(e => e.DCID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.TitleC).HasMaxLength(50);

                entity.Property(e => e.TitleE).HasMaxLength(50);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BC)
                    .WithMany(p => p.D_Company)
                    .HasForeignKey(d => d.BCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_D_Company_B_Category");
            });

            modelBuilder.Entity<D_Department>(entity =>
            {
                entity.HasKey(e => e.DDID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.TitleC).HasMaxLength(50);

                entity.Property(e => e.TitleE).HasMaxLength(50);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.DC)
                    .WithMany(p => p.D_Department)
                    .HasForeignKey(d => d.DCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_D_Department_D_Company");
            });

            modelBuilder.Entity<D_FoodCategory>(entity =>
            {
                entity.HasKey(e => e.DFCID);

                entity.Property(e => e.BOCID).HasDefaultValueSql("((1))");

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(60);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BOC)
                    .WithMany(p => p.D_FoodCategory)
                    .HasForeignKey(d => d.BOCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_D_FoodCategory_B_OrderCode");
            });

            modelBuilder.Entity<D_Hall>(entity =>
            {
                entity.HasKey(e => e.DHID);

                entity.Property(e => e.BusinessTaxRatePercentage).HasComment("營業稅率（百分比）");

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.TitleC).HasMaxLength(50);

                entity.Property(e => e.TitleE).HasMaxLength(50);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.DD)
                    .WithMany(p => p.D_Hall)
                    .HasForeignKey(d => d.DDID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_D_Hall_D_Department");
            });

            modelBuilder.Entity<D_OtherPayItem>(entity =>
            {
                entity.HasKey(e => e.DOPIID);

                entity.Property(e => e.BOCID).HasDefaultValueSql("((1))");

                entity.Property(e => e.BSCID).HasDefaultValueSql("((1))");

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(60);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<D_PayType>(entity =>
            {
                entity.HasKey(e => e.DPTID);

                entity.Property(e => e.AccountingNo)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.CustormerNo)
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.Title).HasMaxLength(60);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BC)
                    .WithMany(p => p.D_PayType)
                    .HasForeignKey(d => d.BCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_D_PayType_B_Category");
            });

            modelBuilder.Entity<D_TimeSpan>(entity =>
            {
                entity.HasKey(e => e.DTSID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(60);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<D_Zip>(entity =>
            {
                entity.HasKey(e => e.DZID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.GroupName).HasMaxLength(100);

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<GroupData>(entity =>
            {
                entity.HasKey(e => e.GID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<M_Contect>(entity =>
            {
                entity.HasKey(e => e.MID);

                entity.Property(e => e.ContectData).HasMaxLength(30);

                entity.Property(e => e.TargetTable)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<M_Customer_BusinessUser>(entity =>
            {
                entity.HasKey(e => e.MID)
                    .HasName("M_Customer_BusinessUser_pk");

                entity.HasIndex(e => e.MID)
                    .HasName("M_Customer_BusinessUser_MID_uindex")
                    .IsUnique();

                entity.Property(e => e.MID).HasComment("流水號 ID");

                entity.Property(e => e.ActiveFlag).HasComment("是否啟用");

                entity.Property(e => e.BUID).HasComment("業務負責人 ID");

                entity.Property(e => e.CID).HasComment("客戶 ID");

                entity.Property(e => e.CreDate)
                    .HasColumnType("datetime")
                    .HasComment("建立時間");

                entity.Property(e => e.CreUID).HasComment("建立者 ID");

                entity.Property(e => e.DeleteFlag).HasComment("是否移除");

                entity.Property(e => e.MappingType).HasComment("對應模式(0:無指定/1:MK/2:OP)");

                entity.Property(e => e.SortNo).HasComment("排序");

                entity.Property(e => e.UpdDate)
                    .HasColumnType("datetime")
                    .HasComment("更新時間");

                entity.Property(e => e.UpdUID).HasComment("更新者 ID");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BU)
                    .WithMany(p => p.M_Customer_BusinessUser)
                    .HasForeignKey(d => d.BUID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "M_Customer_BusinessUser_BusinessUser_BUID_fk");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.C)
                    .WithMany(p => p.M_Customer_BusinessUser)
                    .HasForeignKey(d => d.CID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "M_Customer_BusinessUser_Customer_CID_fk");
            });

            modelBuilder.Entity<M_Customer_Category>(entity =>
            {
                entity.HasKey(e => e.MID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BC)
                    .WithMany(p => p.M_Customer_Category)
                    .HasForeignKey(d => d.BCID), "FK_M_Customer_Category_B_Category");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.C)
                    .WithMany(p => p.M_Customer_Category)
                    .HasForeignKey(d => d.CID), "FK_M_Customer_Category_Customer");
            });

            modelBuilder.Entity<M_Department_Category>(entity =>
            {
                entity.HasKey(e => e.MID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BC)
                    .WithMany(p => p.M_Department_Category)
                    .HasForeignKey(d => d.BCID), "FK_M_Department_Category_B_Category");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.DD)
                    .WithMany(p => p.M_Department_Category)
                    .HasForeignKey(d => d.DDID), "FK_M_Department_Category_D_Department");
            });

            modelBuilder.Entity<M_Group_Menu>(entity =>
            {
                entity.HasKey(e => e.MID);

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.G)
                    .WithMany(p => p.M_Group_Menu)
                    .HasForeignKey(d => d.GID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_M_Group_Menu_GroupData");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.MD)
                    .WithMany(p => p.M_Group_Menu)
                    .HasForeignKey(d => d.MDID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_M_Group_Menu_MenuData");
            });

            modelBuilder.Entity<M_Group_User>(entity =>
            {
                entity.HasKey(e => e.MID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.G)
                    .WithMany(p => p.M_Group_User)
                    .HasForeignKey(d => d.GID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_M_Group_User_GroupData");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.U)
                    .WithMany(p => p.M_Group_User)
                    .HasForeignKey(d => d.UID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_M_Group_User_UserData");
            });

            modelBuilder.Entity<M_Resver_TimeSpan>(entity =>
            {
                entity.HasKey(e => e.MID)
                    .HasName("PK_M_ResverSiteTimeSpan");

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.TargetTable)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.DTS)
                    .WithMany(p => p.M_Resver_TimeSpan)
                    .HasForeignKey(d => d.DTSID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_M_ResverSiteTimeSpan_D_TimeSpan");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.RH)
                    .WithMany(p => p.M_Resver_TimeSpan)
                    .HasForeignKey(d => d.RHID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_M_Resver_TimeSpan_Resver_Head");
            });

            modelBuilder.Entity<M_SiteGroup>(entity =>
            {
                entity.HasKey(e => e.MID)
                    .HasName("M_SiteGroup_pk");

                RelationalEntityTypeBuilderExtensions.HasComment((EntityTypeBuilder)entity, "場地組合");

                entity.HasIndex(e => e.MID)
                    .HasName("M_SiteGroup_MID_uindex")
                    .IsUnique();

                entity.Property(e => e.MID).HasComment("流水號 ID");

                entity.Property(e => e.ActiveFlag).HasComment("是否啟用");

                entity.Property(e => e.CreDate)
                    .HasColumnType("datetime")
                    .HasComment("建立時間");

                entity.Property(e => e.CreUID).HasComment("建立者 ID");

                entity.Property(e => e.DeleteFlag).HasComment("是否移除");

                entity.Property(e => e.GroupID).HasComment("串聯的場地 ID");

                entity.Property(e => e.MasterID).HasComment("場地 ID");

                entity.Property(e => e.SortNo).HasComment("排序");

                entity.Property(e => e.UpdDate)
                    .HasColumnType("datetime")
                    .HasComment("更新時間");

                entity.Property(e => e.UpdUID).HasComment("更新者 ID");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.Group)
                    .WithMany(p => p.M_SiteGroupGroup)
                    .HasForeignKey(d => d.GroupID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "M_SiteGroup_B_SiteData_BSID_fk_2");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.Master)
                    .WithMany(p => p.M_SiteGroupMaster)
                    .HasForeignKey(d => d.MasterID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "M_SiteGroup_B_SiteData_BSID_fk");
            });

            modelBuilder.Entity<MenuAPI>(entity =>
            {
                entity.HasKey(e => e.SeqNo);

                entity.Property(e => e.APIURL).HasMaxLength(100);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.MD)
                    .WithMany(p => p.MenuAPI)
                    .HasForeignKey(d => d.MDID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_MenuAPI_MenuData");
            });

            modelBuilder.Entity<MenuData>(entity =>
            {
                entity.HasKey(e => e.MDID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(50);

                entity.Property(e => e.URL).HasMaxLength(300);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Resver_Bill>(entity =>
            {
                entity.HasKey(e => e.RBID)
                    .HasName("PK_Resver_Bill_Header");

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.PayDate).HasColumnType("datetime");

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BC)
                    .WithMany(p => p.Resver_Bill)
                    .HasForeignKey(d => d.BCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Bill_Header_B_Category");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.DPT)
                    .WithMany(p => p.Resver_Bill)
                    .HasForeignKey(d => d.DPTID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Bill_Header_D_PayType");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.RH)
                    .WithMany(p => p.Resver_Bill)
                    .HasForeignKey(d => d.RHID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Bill_Header_Resver_Head");
            });

            modelBuilder.Entity<Resver_Device>(entity =>
            {
                entity.HasKey(e => e.RDID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.PrintNote).HasMaxLength(100);

                entity.Property(e => e.PrintTitle).HasMaxLength(100);

                entity.Property(e => e.TargetDate).HasColumnType("datetime");

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BD)
                    .WithMany(p => p.Resver_Device)
                    .HasForeignKey(d => d.BDID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Device_B_Device");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BOC)
                    .WithMany(p => p.Resver_Device)
                    .HasForeignKey(d => d.BOCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Device_B_OrderCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.RS)
                    .WithMany(p => p.Resver_Device)
                    .HasForeignKey(d => d.RSID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Device_Resver_Site");
            });

            modelBuilder.Entity<Resver_GiveBack>(entity =>
            {
                entity.HasKey(e => e.RGBID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.PointDecimal).HasComment("給分（十分比）");

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.RH)
                    .WithMany(p => p.Resver_GiveBack)
                    .HasForeignKey(d => d.RHID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_GiveBack_Resver_Head");
            });

            modelBuilder.Entity<Resver_Head>(entity =>
            {
                entity.HasKey(e => e.RHID);

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.ContactName).HasMaxLength(50);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.CustomerTitle).HasMaxLength(100);

                entity.Property(e => e.EDate).HasColumnType("datetime");

                entity.Property(e => e.MK_Phone).HasMaxLength(50);

                entity.Property(e => e.Note)
                    .HasMaxLength(10)
                    .IsFixedLength();

                entity.Property(e => e.OP_Phone).HasMaxLength(50);

                entity.Property(e => e.SDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSCID11Navigation)
                    .WithMany(p => p.Resver_HeadBSCID11Navigation)
                    .HasForeignKey(d => d.BSCID11)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Head_B_StaticCode11");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSCID12Navigation)
                    .WithMany(p => p.Resver_HeadBSCID12Navigation)
                    .HasForeignKey(d => d.BSCID12)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Head_B_StaticCode12");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.C)
                    .WithMany(p => p.Resver_Head)
                    .HasForeignKey(d => d.CID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "Resver_Head_Customer_CID_fk");
            });

            modelBuilder.Entity<Resver_Other>(entity =>
            {
                entity.HasKey(e => e.ROID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.PrintNote).HasMaxLength(100);

                entity.Property(e => e.PrintTitle).HasMaxLength(100);

                entity.Property(e => e.TargetDate).HasColumnType("datetime");

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BOC)
                    .WithMany(p => p.Resver_Other)
                    .HasForeignKey(d => d.BOCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Other_B_OrderCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.DOPI)
                    .WithMany(p => p.Resver_Other)
                    .HasForeignKey(d => d.DOPIID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Other_D_OtherPayItem");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.RH)
                    .WithMany(p => p.Resver_Other)
                    .HasForeignKey(d => d.RHID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Other_Resver_Head");
            });

            modelBuilder.Entity<Resver_Site>(entity =>
            {
                entity.HasKey(e => e.RSID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.PrintNote).HasMaxLength(100);

                entity.Property(e => e.PrintTitle).HasMaxLength(100);

                entity.Property(e => e.TargetDate).HasColumnType("datetime");

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BOC)
                    .WithMany(p => p.Resver_Site)
                    .HasForeignKey(d => d.BOCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Site_B_OrderCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSC)
                    .WithMany(p => p.Resver_Site)
                    .HasForeignKey(d => d.BSCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Site_B_StaticCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BS)
                    .WithMany(p => p.Resver_Site)
                    .HasForeignKey(d => d.BSID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Site_B_SiteData");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.RH)
                    .WithMany(p => p.Resver_Site)
                    .HasForeignKey(d => d.RHID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Site_Resver_Head");
            });

            modelBuilder.Entity<Resver_Throw>(entity =>
            {
                entity.HasKey(e => e.RTID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.PrintNote).HasMaxLength(100);

                entity.Property(e => e.PrintTitle).HasMaxLength(100);

                entity.Property(e => e.TargetDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(100);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BOC)
                    .WithMany(p => p.Resver_Throw)
                    .HasForeignKey(d => d.BOCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Throw_B_OrderCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSC)
                    .WithMany(p => p.Resver_Throw)
                    .HasForeignKey(d => d.BSCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Throw_B_StaticCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.RS)
                    .WithMany(p => p.Resver_Throw)
                    .HasForeignKey(d => d.RSID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Throw_Resver_Site");
            });

            modelBuilder.Entity<Resver_Throw_Food>(entity =>
            {
                entity.HasKey(e => e.RTFID)
                    .HasName("PK_Resver_Throw_D");

                entity.Property(e => e.RTFID).ValueGeneratedNever();

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BP)
                    .WithMany(p => p.Resver_Throw_Food)
                    .HasForeignKey(d => d.BPID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Throw_D_B_Partner");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.BSC)
                    .WithMany(p => p.Resver_Throw_Food)
                    .HasForeignKey(d => d.BSCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Throw_D_B_StaticCode");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.DFC)
                    .WithMany(p => p.Resver_Throw_Food)
                    .HasForeignKey(d => d.DFCID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Throw_D_D_FoodCategory");

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.RT)
                    .WithMany(p => p.Resver_Throw_Food)
                    .HasForeignKey(d => d.RTID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_Resver_Throw_D_Resver_Throw");
            });

            modelBuilder.Entity<UserData>(entity =>
            {
                entity.HasKey(e => e.UID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.DDID).HasDefaultValueSql("((1))");

                entity.Property(e => e.LoginAccount)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.LoginDate).HasColumnType("datetime");

                entity.Property(e => e.LoginPassword)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.UpdDate).HasColumnType("datetime");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(50);

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.DD)
                    .WithMany(p => p.UserData)
                    .HasForeignKey(d => d.DDID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_UserData_D_Department");
            });

            modelBuilder.Entity<UserLog>(entity =>
            {
                entity.HasKey(e => e.ULID);

                entity.Property(e => e.CreDate).HasColumnType("datetime");

                entity.Property(e => e.RequestUrl).HasComment("該次要求的網址");

                entity.Property(e => e.TargetTable)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                RelationalForeignKeyBuilderExtensions.HasConstraintName((ReferenceCollectionBuilder)entity.HasOne(d => d.U)
                    .WithMany(p => p.UserLog)
                    .HasForeignKey(d => d.UID)
                    .OnDelete(DeleteBehavior.ClientSetNull), "FK_UserLog_UserData");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
