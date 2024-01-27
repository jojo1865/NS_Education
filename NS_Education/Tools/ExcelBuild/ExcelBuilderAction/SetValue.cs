using System;
using NPOI.SS.UserModel;

namespace NS_Education.Tools.ExcelBuild.ExcelBuilderAction
{
    /// <summary>
    /// 在 ExcelBuilder 中，對於單一資料格設值的行為。
    /// </summary>
    public class SetValue<TValue> : IExcelBuilderAction
    {
        /// <summary>
        /// 資料格的編號（column）。0-Indexed。
        /// </summary>
        public int CellNo { get; set; }

        /// <summary>
        /// 資料格的類型。
        /// </summary>
        public CellType CellType { get; set; } = CellType.String;

        /// <summary>
        /// 欲設定的值。
        /// </summary>
        public TValue Value { get; set; }

        /// <inheritdoc />
        public void Execute(IRow row)
        {
            ICell cell = row.GetCell(CellNo) ?? row.CreateCell(CellNo, CellType);

            switch (CellType)
            {
                case CellType.Unknown:
                    cell.SetCellValue(Convert.ToString(Value));
                    break;
                case CellType.Numeric:
                    cell.SetCellValue(Convert.ToDouble(Value));
                    break;
                case CellType.String:
                    cell.SetCellValue(Convert.ToString(Value));
                    break;
                case CellType.Formula:
                    cell.SetCellFormula(Convert.ToString(Value));
                    break;
                case CellType.Blank:
                    break;
                case CellType.Boolean:
                    cell.SetCellValue(Convert.ToBoolean(Value));
                    break;
                case CellType.Error:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}