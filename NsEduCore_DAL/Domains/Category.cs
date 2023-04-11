using System;

namespace NsEduCore_DAL.Domains
{
    public class Category
    {
        public int BCID { get; set; }
        public int CategoryType { get; set; }
        public int ParentID { get; set; }
        public string Code { get; set; }
        public string TitleC { get; set; }
        public string TitleE { get; set; }
        public int SortNo { get; set; }
        public bool ActiveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime CreDate { get; set; }
        public int CreUID { get; set; }
        public DateTime UpdDate { get; set; }
        public int UpdUID { get; set; }
    }
}