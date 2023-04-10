using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class UserData
    {
        public UserData()
        {
            M_Group_User = new HashSet<M_Group_User>();
            UserLog = new HashSet<UserLog>();
        }

        public int UID { get; set; }
        public string UserName { get; set; }
        public string LoginAccount { get; set; }
        public string LoginPassword { get; set; }
        public string Note { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }
        public DateTime LoginDate { get; set; }

        public virtual ICollection<M_Group_User> M_Group_User { get; set; }
        public virtual ICollection<UserLog> UserLog { get; set; }
    }
}
