using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
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

        public virtual Customer CIDNavigation { get; set; }
    }
}
