namespace NS_Education.Models.APIItems.Controller.PrintReport.Report3
{
    public class Report3_Output_Row_Income_APIItem
    {
        public string Title { get; set; }
        public string Note { get; set; }

        /// <summary>
        /// 定價
        /// </summary>
        public int FixedPrice { get; set; }

        /// <summary>
        /// 報價
        /// </summary>
        public int QuotedPrice { get; set; }

        /// <summary>
        /// 單位成本
        /// </summary>
        public int UnitPrice { get; set; }

        /// <summary>
        /// 價差
        /// </summary>
        public int Difference => QuotedPrice - UnitPrice;
    }
}