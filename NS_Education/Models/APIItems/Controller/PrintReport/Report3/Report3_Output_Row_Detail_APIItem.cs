using NS_Education.Tools.Extensions;

namespace NS_Education.Models.APIItems.Controller.PrintReport.Report3
{
    public class Report3_Output_Row_Detail_APIItem
    {
        private string _overrideColumnTypeName = null;
        public string TypeName { get; set; }

        /// <summary>
        /// （可選）在輸出報表時，指定是否覆寫細項說明表格中的欄位名稱（例如其他收費項目，欄位需顯示成收費項目，與 TypeName 不同）
        /// </summary>
        public string OverrideColumnTypeName
        {
            get => _overrideColumnTypeName.HasContent() ? _overrideColumnTypeName : TypeName;
            set => _overrideColumnTypeName = value;
        }

        public string Date { get; set; }
        public string[] TimeSpans { get; set; }
        public string Title { get; set; }
        public string SubTypeName { get; set; }
        public string SubType { get; set; }
        public int FixedPrice { get; set; }
        public int QuotedPrice { get; set; }
    }
}