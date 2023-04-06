using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NS_Education.Models
{
    public class D_Company_APIItem
    {
        public int DCID { get; set; }
        public int BCID { get; set; }
        public string BC_TitleC { get; set; }
        public string BC_TitleE { get; set; }
        public List<cSelectItem> CategoryList { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int DepartmentCt { get; set; }
        public bool ActiveFlag { get; set; }
        public string CreDate { get; set; }
        public string CreUser { get; set; }
        public int CreUID { get; set; }
        public string UpdDate { get; set; }
        public string UpdUser { get; set; }
        public int UpdUID { get; set; }
    }
}