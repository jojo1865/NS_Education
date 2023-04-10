using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
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
