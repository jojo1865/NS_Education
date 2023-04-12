

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
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

        public virtual GroupData G { get; set; }
        public virtual MenuData MD { get; set; }
    }
}
