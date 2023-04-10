using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class M_Group_Menu
    {
        public int MID { get; set; }
        public int GID { get; set; }
        public int MDID { get; set; }
        public bool ShowFlag { get; set; }
        public bool AddFlag { get; set; }
        public bool EditFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public bool PringFlag { get; set; }

        public virtual GroupData GIDNavigation { get; set; }
        public virtual MenuData MD { get; set; }
    }
}
