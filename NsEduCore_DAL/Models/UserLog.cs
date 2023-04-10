using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class UserLog
    {
        public long ULID { get; set; }
        public int UID { get; set; }
        public string TargetTable { get; set; }
        public int TargetID { get; set; }
        public int ControlType { get; set; }
        public DateTime CreDate { get; set; }

        public virtual UserData UIDNavigation { get; set; }
    }
}
