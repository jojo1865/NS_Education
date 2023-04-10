using System;
using System.Collections.Generic;

#nullable disable

namespace NsEduCore_DAL.Models
{
    public partial class D_Zip
    {
        public int DZID { get; set; }
        public int ParentID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string GroupName { get; set; }
        public string Note { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }
    }
}
