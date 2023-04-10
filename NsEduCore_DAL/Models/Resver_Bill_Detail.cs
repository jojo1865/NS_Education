using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class Resver_Bill_Detail
    {
        public int RBDID { get; set; }
        public int RBHID { get; set; }
        public string TargetTable { get; set; }
        public int TargetID { get; set; }
        public int Price { get; set; }
        public string Note { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual Resver_Bill_Header RBH { get; set; }
    }
}
