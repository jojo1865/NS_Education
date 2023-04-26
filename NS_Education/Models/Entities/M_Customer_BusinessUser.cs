using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class M_Customer_BusinessUser
    {
        public int MID { get; set; }
        public int CID { get; set; }
        public int BUID { get; set; }
        public int MappingType { get; set; }
        public int SortNo { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual BusinessUser BU { get; set; }
        public virtual Customer C { get; set; }
    }
}
