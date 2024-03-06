using NPOI.SS.UserModel;
using NS_Education.Tools.ExcelBuild.ExcelBuilderAction;

namespace NS_Education.Tools.ExcelBuild
{
    /// <summary>
    /// ExcelBuilder 中表達一個橫列的物件。
    /// </summary>
    internal class ExcelBuilderRow
    {
        #region Initialization

        /// <summary>
        /// 對應報表中的第幾列。0-Indexed。
        /// </summary>
        public int RowNo { get; set; }

        #endregion

        #region Private Functions

        private IRow GetRow(ISheet sheet)
        {
            return sheet.GetRow(RowNo) ?? sheet.CreateRow(RowNo);
        }

        #endregion

        #region Internal Functions

        internal void ExecuteAction(ISheet sheet, IExcelBuilderAction action)
        {
            IRow row = GetRow(sheet);

            action.Execute(row);
        }

        #endregion

        public void AutoHeight(ISheet sheet)
        {
            GetRow(sheet).Height = -1;
        }
    }
}