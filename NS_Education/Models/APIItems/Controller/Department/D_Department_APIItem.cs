using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.Department
{
    public class D_Department_List
    {
        public D_Department_List() { }
        public bool SuccessFlag { get; set; }
        public string Message { get; set; }
        public int NowPage { get; set; }
        public int CutPage { get; set; }
        public int AllItemCt { get; set; }
        public int AllPageCt { get; set; }
        public List<D_Department_APIItem> Items { get; set; }
    }
    public class D_Department_APIItem
    {
        public int DDID { get; set; }
        public int DCID { get; set; }
        public string DC_TitleC { get; set; }
        public string DC_TitleE { get; set; }
        public List<cSelectItem> DC_List { get; set; }
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