using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class D_TimeSpan
    {
        public D_TimeSpan()
        {
            Resver_DeviceDTSIDENavigation = new HashSet<Resver_Device>();
            Resver_DeviceDTSIDSNavigation = new HashSet<Resver_Device>();
            Resver_SiteDTSIDENavigation = new HashSet<Resver_Site>();
            Resver_SiteDTSIDSNavigation = new HashSet<Resver_Site>();
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

        public virtual ICollection<Resver_Device> Resver_DeviceDTSIDENavigation { get; set; }
        public virtual ICollection<Resver_Device> Resver_DeviceDTSIDSNavigation { get; set; }
        public virtual ICollection<Resver_Site> Resver_SiteDTSIDENavigation { get; set; }
        public virtual ICollection<Resver_Site> Resver_SiteDTSIDSNavigation { get; set; }
    }
}
