using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.DataValidationErrors
{
    public sealed class DataNotFoundError : BaseDataValidationError
    {
        public DataNotFoundError()
        {
        }

        public DataNotFoundError(string fieldNameChinese, string fieldName)
        {
            FieldNameChinese = fieldNameChinese;
            FieldName = fieldName;
        }

        public string FieldName { get; }
        public string FieldNameChinese { get; }

        public override int ErrorCodeInt => 1;
        public override string ErrorMessage => GetMessage();

        private string GetMessage()
        {
            return $"{FieldNameChinese?.UnicodeQuote() ?? ""}查無資料！";
        }
    }
}