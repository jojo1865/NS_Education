using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class B_Device
    {
        public B_Device()
        {
            Resver_Device = new HashSet<Resver_Device>();
        }

        public int BDID { get; set; }
        public int BCID { get; set; }
        public int BSCID { get; set; }
        public int BOCID { get; set; }
        public int DHID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public string SupplierTitle { get; set; }
        public string SupplierName { get; set; }
        public string SupplierPhone { get; set; }
        public string Repair { get; set; }
        public string Note { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_Category BC { get; set; }
        public virtual B_OrderCode BOC { get; set; }
        public virtual B_StaticCode BSC { get; set; }
        public virtual D_Hall DH { get; set; }
        public virtual ICollection<Resver_Device> Resver_Device { get; set; }
    }
}
