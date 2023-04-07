using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NS_Education.Models
{
    public class D_Zip_List
    {
        public D_Zip_List() { }
        public bool SuccessFlag { get; set; }
        public string Message { get; set; }
        public int NowPage { get; set; }
        public int CutPage { get; set; }
        public int AllItemCt { get; set; }
        public int AllPageCt { get; set; }
        public List<D_Zip_APIItem> Items { get; set; }
    }
    public class D_Zip_APIItem
    {
        public int DZID { get; set; }
        public int ParentID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string GroupName { get; set; }
        public string Note { get; set; }
        public bool ActiveFlag { get; set; }
        public string CreDate { get; set; }
        public string CreUser { get; set; }
        public int CreUID { get; set; }
        public string UpdDate { get; set; }
        public string UpdUser { get; set; }
        public int UpdUID { get; set; }
    }
}