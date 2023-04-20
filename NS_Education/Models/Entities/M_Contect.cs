

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class M_Contect
    {
        public int MID { get; set; }
        public int ContectType { get; set; }
        public string TargetTable { get; set; }
        public int TargetID { get; set; }
        public string ContectData { get; set; }
        public int SortNo { get; set; }
    }
}
