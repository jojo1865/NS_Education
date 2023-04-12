using System.Collections.Generic;

namespace NS_Education.Models
{
    public class B_Category_List
    {
        public B_Category_List() { }
        public bool SuccessFlag { get; set; }
        public string Message { get; set; }
        public int NowPage { get; set; }
        public int CutPage { get; set; }
        public int AllItemCt { get; set; }
        public int AllPageCt { get; set; }
        public List<B_Category_APIItem> Items { get; set; }
    }
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