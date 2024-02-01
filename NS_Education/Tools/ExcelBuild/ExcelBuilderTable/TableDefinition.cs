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

        private bool HasTotal => Columns.Any(c => c.HasTotal);

        private void WriteTable(ExcelBuilder builder)
        {
            // 表頭

            builder.CreateRow();

            foreach (ColumnDefinition<TDataRow> c in Columns)
            {
                builder.SetValue(c.CellNo, c.ColumnName);
            }

            builder.DrawBorder(BorderDirection.Bottom);

            // 資料列

            foreach (TDataRow data in DataRows)
            {
                builder.CreateRow();

                foreach (ColumnDefinition<TDataRow> c in Columns)
                {
                    object value = c.ValueSelector(data);

                    builder.SetValue(c.CellNo, c.FormatValue(value), c.DataCellType);

                    if (c.DataAlignment != null)
                        builder.SetAlignment(c.CellNo, c.DataAlignment.Value);
                }
            }

            // 有任何欄位有合計時，輸出合計

            if (!HasTotal) return;

            builder.DrawBorder(BorderDirection.Bottom);

            builder.CreateRow()
                .SetValue(0, "合計");

            foreach (ColumnDefinition<TDataRow> c in Columns.Where(c => c.HasTotal))
            {
                decimal sum = DataRows
                    .Select(d => (decimal?)Convert.ToDecimal(c.ValueSelector(d)))
                    .Sum() ?? 0;

                builder.SetValue(c.CellNo,
                    c.FormatValue(sum),
                    c.DataCellType);

                if (c.DataAlignment != null)
                    builder.SetAlignment(c.CellNo, c.DataAlignment.Value);
            }

            return;
        }

        public TableDefinition<TDataRow> SetDataRows(IEnumerable<TDataRow> rows)
        {
            DataRows = rows;
            return this;
        }

        public TableDefinition<TDataRow> StringColumn(int cellNo, string name, Func<TDataRow, string> valueSelector)
        {
            ColumnDefinition<TDataRow> column = new ColumnDefinition<TDataRow>
            {
                CellNo = cellNo,
                ColumnName = name,
                ValueSelector = valueSelector
            };

            Columns.Add(column);
            return this;
        }

        public TableDefinition<TDataRow> NumberColumn(int cellNo, string name,
            Func<TDataRow, decimal> valueSelector, bool? hasTotal = false)
        {
            ColumnDefinition<TDataRow> column = new ColumnDefinition<TDataRow>
            {
                CellNo = cellNo,
                ColumnName = name,
                ValueSelector = d => valueSelector(d),
                DataCellType = CellType.String,
                DataAlignment = HorizontalAlignment.Right,
                Formatter = "#,##0",
                HasTotal = hasTotal ?? false
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
    }
}