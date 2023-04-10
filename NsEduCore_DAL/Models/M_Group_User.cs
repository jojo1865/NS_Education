using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class M_Group_User
    {
        public int MID { get; set; }
        public int GID { get; set; }
        public int UID { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual GroupData GIDNavigation { get; set; }
        public virtual UserData UIDNavigation { get; set; }
    }
}
