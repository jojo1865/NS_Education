using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NS_Education.Tools.ExcelBuild.ExcelBuilderAction;
using NS_Education.Tools.ExcelBuild.ExcelBuilderTable;
using NS_Education.Tools.Extensions;

namespace NS_Education.Tools.ExcelBuild
{
    /// <summary>
    /// 用於建立 Xlsx 形式的報表。
    /// </summary>
    public class ExcelBuilder
    {
        #region Private Functions

        private ExcelBuilder AddActionToCurrentRow(IExcelBuilderAction action)
        {
            CurrentRow.AddAction(action);

            return this;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// 報表名稱
        /// </summary>
        public string ReportTitle { get; set; } = "";

        /// <summary>
        /// 預期這張報表最少會有的欄位數。
        /// </summary>
        public int Columns { get; set; } = 1;

        #endregion

        #region Private Variables

        private IWorkbook Workbook { get; } = new XSSFWorkbook();

        private ISheet Sheet => Workbook.GetSheet(ReportTitle) ?? Workbook.CreateSheet(ReportTitle);

        private ICollection<ExcelBuilderRow> Rows { get; } = new List<ExcelBuilderRow>();

        private ExcelBuilderRow CurrentRow => Rows.LastOrDefault();

        #endregion

        #region Public Functions

        public ExcelBuilder CreateHeader(ExcelBuilderInfo info)
        {
            // Header 最少需要五欄
            Columns = Math.Max(Columns, 5);

            CreateRow()
                .CombineCells()
                .SetValueFromLeft(0, "南山人壽教育訓練中心")
                .SetAlignment(0, HorizontalAlignment.Center);

            CreateRow()
                .CombineCells()
                .SetValueFromLeft(0, ReportTitle)
                .SetAlignment(0, HorizontalAlignment.Center);

            CreateRow()
                .SetValueFromLeft(0, "製表ID：")
                .SetValueFromLeft(1, info.CreatorId)
                .SetAlignment(1, HorizontalAlignment.Left)
                .SetValueFromRight(2, "製表日：")
                .SetValueFromRight(1, info.CreatedOn.ToFormattedStringDate())
                .SetValueFromRight(0, info.CreatedOn.ToString("HH:mm"));

            CreateRow()
                .SetValueFromLeft(0, "製表者：")
                .SetValueFromLeft(1, info.CreatorName)
                .SetValueFromRight(2, "頁次：")
                .SetValueFromRight(1, $"第 {info.NowPage} / {info.TotalPage} 頁");

            CreateRow();

            return this;
        }

        /// <summary>
        /// 建立一個新的資料列，並將當前的工作區域移動到該列。
        /// </summary>
        /// <returns>this</returns>
        public ExcelBuilder CreateRow()
        {
            ExcelBuilderRow row = new ExcelBuilderRow
            {
                RowNo = CurrentRow?.RowNo + 1 ?? 0
            };

            Rows.Add(row);

            return this;
        }

        /// <summary>
        /// 開始建立表格。
        /// </summary>
        /// <returns>this</returns>
        public TableDefinition<TRow> StartDefineTable<TRow>()
        {
            return new TableDefinition<TRow>();
        }

        /// <summary>
        /// 合併儲存格
        /// </summary>
        /// <param name="start">合併格的最左欄編號（column）。0-Indexed。</param>
        /// <param name="end">合併格的最右欄編號（column）。0-Indexed。</param>
        /// <returns>this</returns>
        public ExcelBuilder CombineCells(int start, int end)
        {
            if (start > end)
                (start, end) = (end, start);

            IExcelBuilderAction action = new CombineCells
            {
                CellNoStart = start,
                CellNoEnd = end
            };

            return AddActionToCurrentRow(action);
        }

        /// <summary>
        /// 合併當前作業行的整行儲存格。
        /// </summary>
        /// <returns>this</returns>
        public ExcelBuilder CombineCells()
        {
            return CombineCells(0, Columns - 1);
        }

        /// <inheritdoc cref="SetValueFromLeft{TValue}"/>
        public ExcelBuilder SetValue<TValue>(int cellNoFromLeft, TValue content, CellType? cellType = null)
        {
            return SetValueFromLeft(cellNoFromLeft, content, cellType);
        }

        /// <summary>
        /// 在指定的範圍畫邊框。
        /// </summary>
        /// <param name="directions">方向的列舉，可以用 | 來多選。</param>
        /// <param name="cellNoStart">最左的儲存格的編號（column）。0-Indexed。</param>
        /// <param name="cellNoEnd">最右的儲存格的編號（column）。0-Indexed。</param>
        /// <param name="verticalBordersOuterOnly">（可選）左右側的邊框是否只在範圍的最左或最右才畫</param>
        /// <returns>this</returns>
        public ExcelBuilder DrawBorder(BorderDirection directions, int cellNoStart, int cellNoEnd,
            bool? verticalBordersOuterOnly = null)
        {
            IExcelBuilderAction action = new DrawBorderLine
            {
                CellNoStart = cellNoStart,
                CellNoEnd = cellNoEnd,
                Top = directions.HasFlag(BorderDirection.Top),
                Bottom = directions.HasFlag(BorderDirection.Bottom),
                Left = directions.HasFlag(BorderDirection.Left),
                Right = directions.HasFlag(BorderDirection.Right),
                VerticalBordersOuterOnly = verticalBordersOuterOnly ?? false
            };

            return AddActionToCurrentRow(action);
        }

        /// <summary>
        /// 在指定的範圍畫邊框，向右畫直到整張表的最後一欄。
        /// </summary>
        /// <param name="directions">方向的列舉，可以用 | 來多選。</param>
        /// <param name="cellNoStart">最左的儲存格的編號（column）。0-Indexed。</param>
        /// <param name="verticalBordersOuterOnly">（可選）左右側的邊框是否只在範圍的最左或最右才畫</param>
        /// <returns>this</returns>
        public ExcelBuilder DrawBorder(BorderDirection directions, int cellNoStart,
            bool? verticalBordersOuterOnly = null)
        {
            return DrawBorder(directions, cellNoStart, Columns - 1, verticalBordersOuterOnly);
        }

        /// <summary>
        /// 在指定的範圍畫邊框，整行生效。
        /// </summary>
        /// <param name="directions">方向的列舉，可以用 | 來多選。</param>
        /// <param name="verticalBordersOuterOnly">（可選）左右側的邊框是否只在範圍的最左或最右才畫</param>
        /// <returns>this</returns>
        public ExcelBuilder DrawBorder(BorderDirection directions, bool? verticalBordersOuterOnly = null)
        {
            return DrawBorder(directions, 0, Columns - 1, verticalBordersOuterOnly);
        }

        /// <summary>
        /// 指定儲存格，從左方起算找到該格後，設定儲存格的值。
        /// </summary>
        /// <param name="cellNoFromLeft">儲存格的編號（column）。0-Indexed。</param>
        /// <param name="content">內容</param>
        /// <param name="cellType">（可選）儲存格的型態</param>
        /// <typeparam name="TValue">內容的 generic type</typeparam>
        /// <returns>this</returns>
        public ExcelBuilder SetValueFromLeft<TValue>(int cellNoFromLeft, TValue content, CellType? cellType = null)
        {
            IExcelBuilderAction action = new SetValue<TValue>
            {
                CellNo = cellNoFromLeft,
                CellType = cellType
                           ?? (Double.TryParse(content.ToString(), out _) ? CellType.Numeric : CellType.String),
                Value = content
            };

            return AddActionToCurrentRow(action);
        }

        /// <summary>
        /// 指定儲存格，從右方起算找到該格後，設定儲存格的值。
        /// </summary>
        /// <param name="cellNoFromRight">儲存格的編號（column）。0-Indexed。0 為這行中最右的一格，1 為它的左邊一格，以此類推。</param>
        /// <param name="content">內容</param>
        /// <param name="cellType">（可選）儲存格的型態</param>
        /// <typeparam name="TValue">內容的 generic type</typeparam>
        /// <returns>this</returns>
        public ExcelBuilder SetValueFromRight<TValue>(int cellNoFromRight, TValue content,
            CellType? cellType = null)
        {
            return SetValueFromLeft(Math.Max(0, Columns - cellNoFromRight - 1), content, cellType);
        }

        /// <summary>
        /// 指定儲存格，從左方起算找到該格後，設定儲存格的對齊。
        /// </summary>
        /// <param name="cellNoFromLeft">儲存個編號（column）。0-Indexed。</param>
        /// <param name="alignment">對齊</param>
        /// <returns>this</returns>
        public ExcelBuilder SetAlignment(int cellNoFromLeft, HorizontalAlignment alignment)
        {
            IExcelBuilderAction action = new SetAlignment
            {
                CellNo = cellNoFromLeft,
                Alignment = alignment
            };

            return AddActionToCurrentRow(action);
        }

        /// <summary>
        /// 產生報表並回傳 XLSX 以供下載。
        /// </summary>
        /// <returns><see cref="FileContentResult"/></returns>
        public FileContentResult GetFile()
        {
            foreach (ExcelBuilderRow excelBuilderRow in Rows)
            {
                excelBuilderRow.ExecuteActions(Sheet);
                excelBuilderRow.SetHeightRatio(Sheet, 1.25f);
            }

            for (int i = 0; i < Columns; i++)
            {
                Sheet.AutoSizeColumn(i, true);
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Workbook.Write(ms);
                return new FileContentResult(ms.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = Path.ChangeExtension(ReportTitle, "xlsx")
                };
            }
        }

        #endregion
    }
}