using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class UserLog
    {
        public long ULID { get; set; }
        public int UID { get; set; }
        public string TargetTable { get; set; }
        public int TargetID { get; set; }
        public int ControlType { get; set; }
        public DateTime CreDate { get; set; }

        public virtual UserData U { get; set; }
    }
}
