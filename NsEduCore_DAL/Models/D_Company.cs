using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class D_Company
    {
        public D_Company()
        {
            D_Department = new HashSet<D_Department>();
        }

        public int DCID { get; set; }
        public int BCID { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual B_Category BC { get; set; }
        public virtual ICollection<D_Department> D_Department { get; set; }
    }
}
