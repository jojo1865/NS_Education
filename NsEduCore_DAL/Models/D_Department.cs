using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class D_Department
    {
        public D_Department()
        {
            D_Hall = new HashSet<D_Hall>();
            M_Department_Category = new HashSet<M_Department_Category>();
        }

        public int DDID { get; set; }
        public int DCID { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int PeopleCt { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual D_Company DC { get; set; }
        public virtual ICollection<D_Hall> D_Hall { get; set; }
        public virtual ICollection<M_Department_Category> M_Department_Category { get; set; }
    }
}
