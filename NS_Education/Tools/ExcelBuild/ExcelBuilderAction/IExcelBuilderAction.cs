using NPOI.SS.UserModel;

namespace NS_Education.Tools.ExcelBuild.ExcelBuilderAction
{
    /// <summary>
    /// ExcelBuilder 中，在單一資料列的脈絡下，表達對於資料格進行的處理的物件。
    /// </summary>
    internal interface IExcelBuilderAction
    {
        void Execute(IRow row);
    }
}