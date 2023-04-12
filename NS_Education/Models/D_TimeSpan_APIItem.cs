using System.Collections.Generic;

namespace NS_Education.Models
{
    public class D_TimeSpan_List
    {
        public D_TimeSpan_List() { }
        public bool SuccessFlag { get; set; }
        public string Message { get; set; }
        public int NowPage { get; set; }
        public int CutPage { get; set; }
        public int AllItemCt { get; set; }
        public int AllPageCt { get; set; }
        public List<D_TimeSpan_APIItem> Items { get; set; }
    }
    public class D_TimeSpan_APIItem
    {
        public int DTSID { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public int HourS { get; set; }
        public int MinuteS { get; set; }
        public int HourE { get; set; }
        public int MinuteE { get; set; }

        public string TimeS { get; set; }
        public string TimeE { get; set; }

        public string GetTimeSpan { get; set; }

        public bool ActiveFlag { get; set; }
        public string CreDate { get; set; }
        public string CreUser { get; set; }
        public int CreUID { get; set; }
        public string UpdDate { get; set; }
        public string UpdUser { get; set; }
        public int UpdUID { get; set; }
    }
}