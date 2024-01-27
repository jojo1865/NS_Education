using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace NS_Education.Tools.ExcelBuild.ExcelBuilderAction
{
    /// <summary>
    /// 在 ExcelBuilder 中，合併資料格的行為。
    /// </summary>
    public class CombineCells : IExcelBuilderAction
    {
        /// <summary>
        /// 合併起點的資料格編號（column）。0-Indexed。
        /// </summary>
        public int CellNoStart { get; set; }

        /// <summary>
        /// 合併終點的資料格編號（column）。0-Indexed。
        /// </summary>
        public int? CellNoEnd { get; set; }

        /// <inheritdoc />
        public void Execute(IRow row)
        {
            row.Sheet.AddMergedRegion(new CellRangeAddress(row.RowNum, row.RowNum, CellNoStart,
                CellNoEnd ?? CellNoStart));
        }
    }
}