using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class D_TimeSpan
    {
        public D_TimeSpan()
        {
            M_ResverSiteTimeSpan = new HashSet<M_ResverSiteTimeSpan>();
            Resver_DeviceDTSIDENavigation = new HashSet<Resver_Device>();
            Resver_DeviceDTSIDSNavigation = new HashSet<Resver_Device>();
        }

        public int DTSID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int HourS { get; set; }
        public int MinuteS { get; set; }
        public int HourE { get; set; }
        public int MinuteE { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual ICollection<M_ResverSiteTimeSpan> M_ResverSiteTimeSpan { get; set; }
        public virtual ICollection<Resver_Device> Resver_DeviceDTSIDENavigation { get; set; }
        public virtual ICollection<Resver_Device> Resver_DeviceDTSIDSNavigation { get; set; }
    }
}
