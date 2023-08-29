using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report9
{
    /// <summary>
    /// 客戶歷史資料報表的輸入物件。
    /// </summary>
    public class Report9_Input_APIItem : BaseRequestForPagedList
    {
        public IEnumerable<int> CID { get; set; }
        public bool Internal { get; set; } = false;
        public bool External { get; set; } = false;
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}