using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class B_PartnerItem
    {
        public int BPIID { get; set; }
        public int BPID { get; set; }
        public int BSCID { get; set; }
        public int BOCID { get; set; }
        public int DHID { get; set; }
        public int Ct { get; set; }
        public int Price { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public int SortNo { get; set; }
        public string Note { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_OrderCode BOC { get; set; }
        public virtual B_Partner BP { get; set; }
        public virtual B_StaticCode BSC { get; set; }
        public virtual D_Hall DH { get; set; }
    }
}
