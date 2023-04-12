using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class MenuAPI
    {
        public int SeqNo { get; set; }
        public int MDID { get; set; }
        public string APIURL { get; set; }
        public int APIType { get; set; }
        public string Note { get; set; }
        public DateTime CreDate { get; set; }

        public virtual MenuData MD { get; set; }
    }
}
