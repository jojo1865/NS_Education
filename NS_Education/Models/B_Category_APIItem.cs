using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NS_Education.Models
{
    
    public class B_Category_APIItem
    {
        public int BCID { get; set; }
        public int iCategoryType { get; set; }
        public string sCategoryType { get; set; }
        public List<cSelectItem> CategoryTypeList { get; set; }
        public List<cSelectItem> ParentList { get; set; }
        public int ParentID { get; set; }
        public string ParentTitleC { get; set; }
        public string ParentTitleE { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int SortNo { get; set; }
        public bool ActiveFlag { get; set; }
        public string CreDate { get; set; }
        public string CreUser { get; set; }
        public int CreUID { get; set; }
        public string UpdDate { get; set; }
        public string UpdUser { get; set; }
        public int UpdUID { get; set; }
    }
}