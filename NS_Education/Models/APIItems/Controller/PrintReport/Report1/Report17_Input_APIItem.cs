namespace NS_Education.Models.APIItems.Controller.PrintReport.Report1
{
    /// <summary>
    /// 產製對帳單的輸入物件。
    /// </summary>
    public class Report17_Input_APIItem
    {
        /// <summary>
        /// 預約單 ID
        /// </summary>
        public int RHID { get; set; }

        /// <summary>
        /// 付款方式（e.g. 現金）
        /// </summary>
        public string PayMethod { get; set; }

        /// <summary>
        /// 付款說明（樣張上藍色手 key 部分）
        /// </summary>
        public string PayDescription { get; set; }
    }
}