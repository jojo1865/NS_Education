using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class M_Customer_Category
    {
        public int MID { get; set; }
        public int? CID { get; set; }
        public int? BCID { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_Category BC { get; set; }
        public virtual Customer CIDNavigation { get; set; }
    }
}
