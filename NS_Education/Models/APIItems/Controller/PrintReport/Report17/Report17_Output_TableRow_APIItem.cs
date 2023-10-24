namespace NS_Education.Models.APIItems.Controller.PrintReport.Report17
{
    public class Report17_Output_TableRow_APIItem
    {
        /// <summary>
        /// yyyy/MM/dd
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// 帳目說明
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 金額
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// （僅在後端使用）報表底部廠商名稱
        /// </summary>
        internal string PartnerName { get; set; }
    }
}