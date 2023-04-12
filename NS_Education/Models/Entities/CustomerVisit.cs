using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class CustomerVisit
    {
        public int CVID { get; set; }
        public int CID { get; set; }
        public int BSCID { get; set; }
        public int BUID { get; set; }
        public string TargetTitle { get; set; }
        public DateTime VisitDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AfterNote { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_StaticCode BSC { get; set; }
        public virtual BusinessUser BU { get; set; }
        public virtual Customer C { get; set; }
    }
}
