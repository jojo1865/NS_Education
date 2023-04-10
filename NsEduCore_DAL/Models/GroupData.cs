using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class GroupData
    {
        public GroupData()
        {
            M_Group_Menu = new HashSet<M_Group_Menu>();
            M_Group_User = new HashSet<M_Group_User>();
        }

        public int GID { get; set; }
        public string Title { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual ICollection<M_Group_Menu> M_Group_Menu { get; set; }
        public virtual ICollection<M_Group_User> M_Group_User { get; set; }
    }
}
