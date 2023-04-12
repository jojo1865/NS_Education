using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace NS_Education.Models.Entities
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
