namespace NS_Education.Models.APIItems.Controller.PrintReport.Report18
{
    /// <summary>
    /// 日誌贈送報表的單筆輸出物件。
    /// </summary>
    public class Report18_Output_Row_APIItem
    {
        /// <summary>
        /// 贈送日期
        /// </summary>
        public string SendDate { get; internal set; }

        /// <summary>
        /// 客戶
        /// </summary>
        public string C_TitleC { get; internal set; }

        /// <summary>
        /// 品項
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// 數量
        /// </summary>
        public int Ct { get; internal set; }
    }
}