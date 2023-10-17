using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report6
{
    /// <summary>
    /// 人次統計表的輸入物件。
    /// </summary>
    public class Report6_Input_APIItem : BaseRequestForPagedList
    {
        public IEnumerable<int> RHID { get; set; }
        public bool Internal { get; set; } = true;
        public bool External { get; set; } = true;
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public string CustomerName { get; set; }

        public int? State { get; set; }
    }
}