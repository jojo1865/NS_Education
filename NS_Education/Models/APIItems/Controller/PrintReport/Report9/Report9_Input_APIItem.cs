using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report9
{
    /// <summary>
    /// 客戶歷史資料報表的輸入物件。
    /// </summary>
    public class Report9_Input_APIItem : BaseRequestForPagedList
    {
        public IEnumerable<int> CID { get; set; }
        public bool? Internal { get; set; }
        public bool? External { get; set; }

        public bool? CommDept { get; set; }

        // 相容前端錯字欄位
        public bool? CommmDept
        {
            set => CommDept = value;
            get => CommDept;
        }

        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public string CustomerName { get; set; }

        public string CustomerCode { get; set; }
        public int? BSCID6 { get; set; }

        public string ContactName { get; set; }
        public string ContactData { get; set; }
    }
}