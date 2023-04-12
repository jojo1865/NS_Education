using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class MenuData
    {
        public MenuData()
        {
            M_Group_Menu = new HashSet<M_Group_Menu>();
            MenuAPI = new HashSet<MenuAPI>();
        }

        public int MDID { get; set; }
        public int ParentID { get; set; }
        public string Title { get; set; }
        public string URL { get; set; }
        public int SortNo { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual ICollection<M_Group_Menu> M_Group_Menu { get; set; }
        public virtual ICollection<MenuAPI> MenuAPI { get; set; }
    }
}
