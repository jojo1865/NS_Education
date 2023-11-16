using System;
using System.Collections.Generic;
using System.Linq;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report17
{
    public class Report17_Output_SubTable_APIItem
    {
        /// <summary>
        /// 區塊標題
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 區塊小計
        /// </summary>
        public int Sum => Rows.Sum(r => (int?)r.Amount) ?? 0;

        /// <summary>
        /// 區塊總報價
        /// </summary>
        public int? QuotedPrice { get; set; }

        /// <summary>
        /// 明細行的集合
        /// </summary>
        public IEnumerable<Report17_Output_TableRow_APIItem> Rows { get; set; } =
            Array.Empty<Report17_Output_TableRow_APIItem>();
    }
}