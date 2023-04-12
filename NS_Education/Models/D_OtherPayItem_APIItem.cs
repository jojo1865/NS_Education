using System.Collections.Generic;

namespace NS_Education.Models
{
    public class D_OtherPayItem_List
    {
        public D_OtherPayItem_List() { }
        public bool SuccessFlag { get; set; }
        public string Message { get; set; }
        public int NowPage { get; set; }
        public int CutPage { get; set; }
        public int AllItemCt { get; set; }
        public int AllPageCt { get; set; }
        public List<D_OtherPayItem_APIItem> Items { get; set; }
    }
    public class D_OtherPayItem_APIItem
    {
        public int DOPIID { get; set; }
        
        public string Code { get; set; }
        public string Title { get; set; }
        public int Ct { get; set; }
        public int UnitPrice { get; set; }
        public int InPrice { get; set; }
        public int OutPrice { get; set; }
        public int PaidType { get; set; }
        
        public bool ActiveFlag { get; set; }
        public string CreDate { get; set; }
        public string CreUser { get; set; }
        public int CreUID { get; set; }
        public string UpdDate { get; set; }
        public string UpdUser { get; set; }
        public int UpdUID { get; set; }
    }
}