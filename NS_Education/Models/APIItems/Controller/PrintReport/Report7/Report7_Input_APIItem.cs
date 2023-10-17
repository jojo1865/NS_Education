using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report7
{
    /// <summary>
    /// 營運報表明細的輸入物件。
    /// </summary>
    public class Report7_Input_APIItem : BaseRequestForPagedList
    {
        public IEnumerable<int> RHID { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string CustomerName { get; set; }

        public int? State { get; set; }
    }
}