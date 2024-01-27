using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace NS_Education.Tools.ExcelBuild.ExcelBuilderAction
{
    /// <summary>
    /// 在 ExcelBuilder 中，對於單一資料格設定對齊選項。
    /// </summary>
    public class SetAlignment : IExcelBuilderAction
    {
        public int CellNo { get; set; }
        public HorizontalAlignment Alignment { get; set; }

        /// <inheritdoc />
        public void Execute(IRow row)
        {
            ICell cell = row.GetCell(CellNo) ?? row.CreateCell(CellNo);

            CellUtil.SetAlignment(cell, Alignment);
        }
    }
}