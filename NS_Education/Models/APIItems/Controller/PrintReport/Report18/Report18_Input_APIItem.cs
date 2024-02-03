namespace NS_Education.Models.APIItems.Controller.PrintReport.Report18
{
    /// <summary>
    /// 產製日誌贈送報表的輸入物件。
    /// </summary>
    public class Report18_Input_APIItem : BaseRequestForPagedList
    {
        /// <summary>
        /// 起日
        /// </summary>
        public string SDate { get; set; }

        /// <summary>
        /// 迄日
        /// </summary>
        public string EDate { get; set; }

        /// <summary>
        /// 客戶名稱
        /// </summary>
        public string CustomerTitleC { get; set; }

        /// <summary>
        /// 贈送年份
        /// </summary>
        public int? SendYear { get; set; }

        /// <summary>
        /// 禮品
        /// </summary>
        public string Keyword { get; set; }
    }
}