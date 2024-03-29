using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report5
{
    /// <summary>
    /// 餐飲明細表的輸入物件。
    /// </summary>
    public class Report5_Input_APIItem : BaseRequestForPagedList
    {
        public IEnumerable<int> RHID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Partner { get; set; }
        public string CustomerName { get; set; }
        public int? State { get; set; }
    }
}