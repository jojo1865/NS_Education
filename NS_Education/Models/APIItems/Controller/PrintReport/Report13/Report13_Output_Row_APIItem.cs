namespace NS_Education.Models.APIItems.Controller.PrintReport.Report13
{
    /// <summary>
    /// 場地預估銷售月報表的單筆輸出物件。
    /// </summary>
    public class Report13_Output_Row_APIItem
    {
        /// <summary>
        /// 場地代號
        /// </summary>
        public string SiteCode { get; set; }

        /// <summary>
        /// 場地名稱
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// 場地價格（單一時段單價）<br/>
        /// 向後相容用，根據後期需求報表確認，場地報價只需要一個欄位就好
        /// </summary>
        public int SiteTimeSpanUnitPrice => SiteQuotedPrice;

        /// <summary>
        /// 場地成本
        /// </summary>
        public int SiteUnitPrice { get; set; }

        /// <summary>
        /// 場地報價
        /// </summary>
        public int SiteQuotedPrice { get; set; }

        /// <summary>
        /// 時段
        /// </summary>
        public string TimeSpan { get; set; }

        /// <summary>
        /// 數量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 總金額（所有時段價格 * 數量總額）
        /// </summary>
        public int TotalPrice => SiteQuotedPrice * Quantity;
    }
}