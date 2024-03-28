using System;
using System.Collections.Generic;
using System.Linq;
using NPOI.SS.UserModel;

namespace NS_Education.Tools.ExcelBuild.ExcelBuilderTable
{
    /// <summary>
    /// 在 ExcelBuilder 中，表達表格的資訊。
    /// </summary>
    public class TableDefinition<TDataRow>
    {
        /// <summary>
        /// 資料欄的定義。
        /// </summary>
        private ICollection<ColumnDefinition<TDataRow>> Columns { get; set; } = new List<ColumnDefinition<TDataRow>>();

        /// <summary>
        /// 資料列。
        /// </summary>
        private IEnumerable<TDataRow> DataRows { get; set; } = Array.Empty<TDataRow>();

        private BorderStyle TopBorderStyle { get; set; } = BorderStyle.Medium;

        private BorderStyle BottomBorderStyle { get; set; } = BorderStyle.Medium;

        private string TotalText { get; set; } = "合計";

        private bool HasTotal => Columns.Any(c => c.HasTotal);

        private void WriteTable(ExcelBuilder builder)
        {
            // 表頭

            builder.CreateRow();

            foreach (ColumnDefinition<TDataRow> c in Columns)
            {
                builder.SetValue(c.CellNo, c.ColumnName);
            }

            builder.DrawBorder(BorderDirection.Bottom, borderStyle: TopBorderStyle);

            // 資料列

            TDataRow lastRow = default;

            foreach (TDataRow data in DataRows)
            {
                builder.CreateRow();

                foreach (ColumnDefinition<TDataRow> c in Columns)
                {
                    object value = c.ValueSelector(data);

                    if (c.BlankIfSameCondition != null && lastRow != null)
                    {
                        if (c.BlankIfSameCondition(lastRow, data))
                        {
                            continue;
                        }
                    }

                    builder.SetValue(c.CellNo, c.FormatValue(value), c.DataCellType);

                    if (c.DataAlignment != null)
                        builder.Align(c.CellNo, c.DataAlignment.Value);
                }

                lastRow = data;
            }

            // 有任何欄位有合計時，輸出合計

            if (!HasTotal) return;

            builder.DrawBorder(BorderDirection.Bottom, borderStyle: BottomBorderStyle);

            builder.CreateRow()
                .SetValue(0, TotalText);

            foreach (ColumnDefinition<TDataRow> c in Columns.Where(c => c.HasTotal))
            {
                decimal sum = 0;

                TDataRow last = default;
                foreach (TDataRow curr in DataRows)
                {
                    if (last != null && c.NotCountAsTotalIfBlank && c.BlankIfSameCondition(last, curr))
                        continue;

                    sum += Convert.ToDecimal(c.ValueSelector(curr));

                    last = curr;
                }

                builder.SetValue(c.CellNo,
                    c.FormatValue(sum),
                    c.DataCellType);

                if (c.DataAlignment != null)
                    builder.Align(c.CellNo, c.DataAlignment.Value);
            }
        }

        public TableDefinition<TDataRow> SetDataRows(IEnumerable<TDataRow> rows)
        {
            DataRows = rows;
            return this;
        }

        public TableDefinition<TDataRow> StringColumn(int cellNo, string name, Func<TDataRow, string> valueSelector,
            Func<TDataRow, TDataRow, bool> blankIfSameCondition = null)
        {
            ColumnDefinition<TDataRow> column = new ColumnDefinition<TDataRow>
            {
                CellNo = cellNo,
                ColumnName = name,
                ValueSelector = valueSelector,
                DataCellType = CellType.String,
                BlankIfSameCondition = blankIfSameCondition
            };

            Columns.Add(column);
            return this;
        }

        public TableDefinition<TDataRow> NumberColumn(int cellNo, string name,
            Func<TDataRow, decimal> valueSelector, bool hasTotal = false,
            Func<TDataRow, TDataRow, bool> blankIfSameCondition = null,
            bool notCountAsTotalIfBlank = false)
        {
            ColumnDefinition<TDataRow> column = new ColumnDefinition<TDataRow>
            {
                CellNo = cellNo,
                ColumnName = name,
                ValueSelector = d => valueSelector(d),
                DataCellType = CellType.String,
                DataAlignment = HorizontalAlignment.Right,
                Formatter = "#,##0",
                BlankIfSameCondition = blankIfSameCondition,
                HasTotal = hasTotal,
                NotCountAsTotalIfBlank = notCountAsTotalIfBlank
            };

            Columns.Add(column);
            return this;
        }

        /// <summary>
        /// 完成定義表格，並將表格加入 ExcelBuilder 中。
        /// </summary>
        /// <param name="builder">builder</param>
        /// <returns>寫入的 ExcelBuilder</returns>
        public ExcelBuilder AddToBuilder(ExcelBuilder builder)
        {
            WriteTable(builder);
            return builder;
        }

        public TableDefinition<TDataRow> SetTopBorder(BorderStyle borderStyle)
        {
            TopBorderStyle = borderStyle;
            return this;
        }

        public TableDefinition<TDataRow> SetBottomBorder(BorderStyle borderStyle)
        {
            BottomBorderStyle = borderStyle;
            return this;
        }

        /// <summary>
        /// 覆寫「合計」的字樣。
        /// </summary>
        public TableDefinition<TDataRow> OverrideTotalText(string totalText)
        {
            TotalText = totalText;
            return this;
        }
    }
}