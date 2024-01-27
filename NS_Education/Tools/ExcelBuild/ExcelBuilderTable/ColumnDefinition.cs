using System;
using NPOI.SS.UserModel;

namespace NS_Education.Tools.ExcelBuild.ExcelBuilderTable
{
    /// <summary>
    /// 在 ExcelBuilder 中，表達表格的欄位資訊。
    /// </summary>
    public class ColumnDefinition<TDataRow>
    {
        /// <summary>
        /// 欄位對應資料表上的第幾欄（Column）。0-Indexed。
        /// </summary>
        public int CellNo { get; set; }

        /// <summary>
        /// 欄位名稱。顯示於表頭。
        /// </summary>
        public string ColumnName { get; set; }

        /// <summary>
        /// （可選）此欄位在資料列中如何對齊。
        /// </summary>
        public HorizontalAlignment? DataAlignment { get; set; }

        /// <summary>
        /// （可選）此欄位在資料列中強制為什麼資料格類型。
        /// </summary>
        public CellType? DataCellType { get; set; }

        /// <summary>
        /// 此欄位從資料 DTO 如何取值。
        /// </summary>
        public Func<TDataRow, object> ValueSelector { get; set; }

        /// <summary>
        /// （可選）取出的值如何格式化為字串。
        /// </summary>
        public string Formatter { get; set; } = null;

        /// <summary>
        /// 此欄位是否有合計。
        /// </summary>
        public bool HasTotal { get; set; }

        /// <summary>
        /// 將值格式化為最後在表中顯示的格式。
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>格式化的字串</returns>
        public string FormatValue(object value)
        {
            return Formatter == null ? value.ToString() : String.Format("{0:" + Formatter + "}", value);
        }
    }
}