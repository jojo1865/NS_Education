using System;
using System.Collections.Generic;
using System.Linq;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report17
{
    /// <summary>
    /// 產製對帳單的回傳物件。
    /// </summary>
    public class Report17_Output_APIItem : BaseInfusable
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

        /// <summary>
        /// 結帳日期（yyyy/MM/dd）
        /// </summary>
        public string AccountDate { get; set; }

        /// <summary>
        /// 聯絡人
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// 結帳客戶名稱
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 課程/活動名稱
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// 中央主表中的區塊
        /// </summary>
        public IEnumerable<Report17_Output_SubTable_APIItem> SubTables { get; set; } =
            Array.Empty<Report17_Output_SubTable_APIItem>();

        /// <summary>
        /// 費用合計
        /// </summary>
        public int TotalAmount => SubTables.Sum(st => (int?)(st.QuotedPrice ?? st.Sum)) ?? 0;

        /// <summary>
        /// 預付金額
        /// </summary>
        public int PrepaidAmount { get; set; }

        /// <summary>
        /// 餘額
        /// </summary>
        public int UnpaidAmount => TotalAmount - PrepaidAmount;

        /// <summary>
        /// 場租折扣
        /// </summary>
        public int SiteDiscount { get; set; }

        /// <summary>
        /// 統一編號
        /// </summary>
        public string Compilation { get; set; }

        /// <summary>
        /// 發票抬頭
        /// </summary>
        public string PrintTitle { get; set; }

        /// <summary>
        /// 底部支付資訊
        /// </summary>
        public IEnumerable<Report17_Output_Payment_APIItem> Payments { get; set; } =
            Array.Empty<Report17_Output_Payment_APIItem>();

        /// <summary>
        /// 底部支付資訊：場地支付資訊
        /// </summary>
        public Report17_Output_Payment_APIItem SitePayments { get; set; }
    }
}