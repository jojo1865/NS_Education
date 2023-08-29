using System.Collections.Generic;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report3
{
    /// <summary>
    /// dummy 物件
    /// </summary>
    public class Report3_Output_Row_APIItem
    {
        /// <summary>
        /// 預約單號 
        /// </summary>
        public int RHID { get; set; }

        /// <summary>
        /// 主辦單位
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// 活動名稱
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// 活動開始日期
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// 活動結束日期
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// 人數
        /// </summary>
        public int PeopleCt { get; set; }

        public IEnumerable<Report3_Output_Row_Detail_APIItem> Details { get; set; } =
            new List<Report3_Output_Row_Detail_APIItem>();

        public IEnumerable<Report3_Output_Row_Income_APIItem> Incomes { get; set; } =
            new List<Report3_Output_Row_Income_APIItem>();
    }
}