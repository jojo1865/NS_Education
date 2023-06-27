using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.DataValidationErrors
{
    public sealed class DataAlreadyExistsError : BaseDataValidationError
    {
        public DataAlreadyExistsError()
        {
        }

        public DataAlreadyExistsError(string fieldNameChinese, string fieldName)
        {
            FieldNameChinese = fieldNameChinese;
            FieldName = fieldName;
        }

        public string FieldName { get; }
        public string FieldNameChinese { get; }

        public override int ErrorCodeInt => 3;
        public override string ErrorMessage => $"已存在相同{FieldNameChinese?.UnicodeQuote() ?? ""}的資料！";
    }
}