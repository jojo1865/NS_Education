using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class CustomerGift
    {
        public int CGID { get; set; }
        public int CID { get; set; }
        public int Year { get; set; }
        public DateTime SendDate { get; set; }
        public int BSCID { get; set; }
        public string Title { get; set; }
        public int Ct { get; set; }
        public string Note { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual Customer CIDNavigation { get; set; }
    }
}
