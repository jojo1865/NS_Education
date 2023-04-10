using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class D_FoodCategory
    {
        public D_FoodCategory()
        {
            Resver_Throw_Food = new HashSet<Resver_Throw_Food>();
        }

        public int DFCID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int UnitPrice { get; set; }
        public int Price { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }

        public virtual ICollection<Resver_Throw_Food> Resver_Throw_Food { get; set; }
    }
}
