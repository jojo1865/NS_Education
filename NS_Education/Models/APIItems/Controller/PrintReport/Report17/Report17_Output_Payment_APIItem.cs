namespace NS_Education.Models.APIItems.Controller.PrintReport.Report17
{
    public class Report17_Output_Payment_APIItem
    {
        /// <summary>
        /// 支付種類
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 支付金額
        /// </summary>
        public int Amount { get; set; }

        /// <summary>
        /// 支付對象廠商名稱
        /// </summary>
        public string PartnerName { get; set; }
    }
}