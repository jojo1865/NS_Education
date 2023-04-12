using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class Resver_GiveBack
    {
        public int RGBID { get; set; }
        public int RHID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public double Point { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual Resver_Head RH { get; set; }
    }
}
