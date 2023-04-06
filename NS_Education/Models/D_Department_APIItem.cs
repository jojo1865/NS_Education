using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NS_Education.Models
{
    public class D_Department_APIItem
    {
        public int DDID { get; set; }
        public int DCID { get; set; }
        public string DC_TitleC { get; set; }
        public string DC_TitleE { get; set; }
        public List<cSelectItem> CompanyList { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int PeopleCt { get; set; }
        public int HallCt { get; set; }
        public bool ActiveFlag { get; set; }
        public string CreDate { get; set; }
        public string CreUser { get; set; }
        public int CreUID { get; set; }
        public string UpdDate { get; set; }
        public string UpdUser { get; set; }
        public int UpdUID { get; set; }
    }
}