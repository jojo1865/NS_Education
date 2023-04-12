using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class Resver_Device
    {
        public int RDID { get; set; }
        public int RHID { get; set; }
        public DateTime TargetDate { get; set; }
        public int DTSIDS { get; set; }
        public int DTSIDE { get; set; }
        public int BSID { get; set; }
        public int BDID { get; set; }
        public int BOCID { get; set; }
        public string PrintTitle { get; set; }
        public string PrintNote { get; set; }
        public int UnitPrice { get; set; }
        public int Pricing { get; set; }
        public int Ct { get; set; }
        public int TotalPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_Device BD { get; set; }
        public virtual B_OrderCode BOC { get; set; }
        public virtual B_SiteData BS { get; set; }
        public virtual D_TimeSpan DTSIDENavigation { get; set; }
        public virtual D_TimeSpan DTSIDSNavigation { get; set; }
        public virtual Resver_Head RH { get; set; }
    }
}
