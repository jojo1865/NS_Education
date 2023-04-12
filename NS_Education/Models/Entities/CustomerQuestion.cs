using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
{
    public partial class CustomerQuestion
    {
        public int CQID { get; set; }
        public int CID { get; set; }
        public DateTime AskDate { get; set; }
        public string AskTitle { get; set; }
        public string AskArea { get; set; }
        public string AskDescription { get; set; }
        public bool ResponseFlag { get; set; }
        public string ResponseUser { get; set; }
        public string ResponseDestriotion { get; set; }
        public DateTime ResponseDate { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual Customer C { get; set; }
    }
}
