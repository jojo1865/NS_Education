using System.Linq;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NS_Education.Tools.Extensions;

namespace NS_Education.Tools.ExcelBuild.ExcelBuilderAction
{
    /// <summary>
    /// 在 ExcelBuilder 中，對於資料格畫線的行為。
    /// </summary>
    public class DrawBorderLine : IExcelBuilderAction
    {
        /// <summary>
        /// 開始畫線的資料格編號（column）。0-Indexed。
        /// </summary>
        public int CellNoStart { get; set; }

        /// <summary>
        /// 最後一格畫線的資料格編號（column）。0-Indexed。
        /// </summary>
        public int? CellNoEnd { get; set; }

        /// <summary>
        /// 設定邊框線的樣式
        /// </summary>
        public BorderStyle BorderStyle { get; set; } = BorderStyle.Medium;

        /// <summary>
        /// 上方邊線
        /// </summary>
        public bool Top { get; set; } = false;

        /// <summary>
        /// 下方邊線
        /// </summary>
        public bool Bottom { get; set; } = false;

        /// <summary>
        /// 左方邊線
        /// </summary>
        public bool Left { get; set; } = false;

        /// <summary>
        /// 右方邊線
        /// </summary>
        public bool Right { get; set; } = false;

        /// <summary>
        /// 左右的邊框是否只畫在整個範圍的最左與最右
        /// </summary>
        public bool VerticalBordersOuterOnly { get; set; } = false;

        /// <inheritdoc />
        public void Execute(IRow row)
        {
            string[] propertyNames = new[]
                {
                    Top ? CellUtil.BORDER_TOP : null,
                    Bottom ? CellUtil.BORDER_BOTTOM : null,
                    Left ? CellUtil.BORDER_LEFT : null,
                    Right ? CellUtil.BORDER_RIGHT : null
                }
                .Where(s => s.HasContent())
                .ToArray();

            CellNoEnd = CellNoEnd ?? CellNoStart;

            for (int i = CellNoStart; i <= CellNoEnd; i++)
            {
                ICell cell = row.GetCell(i) ?? row.CreateCell(i);

                foreach (string propertyName in propertyNames)
                {
                    // OuterOnly 時...
                    // TOP: 照畫
                    // BOTTOM: 照畫
                    // LEFT: 只在最左
                    // RIGHT: 只在最右

                    if (VerticalBordersOuterOnly)
                    {
                        if (propertyName == CellUtil.BORDER_LEFT && i != CellNoStart)
                            continue;

                        if (propertyName == CellUtil.BORDER_RIGHT && i != CellNoEnd)
                            continue;
                    }

                    CellUtil.SetCellStyleProperty(cell, propertyName, BorderStyle);
                }
            }
        }
    }
}