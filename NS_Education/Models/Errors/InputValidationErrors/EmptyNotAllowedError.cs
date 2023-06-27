using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class EmptyNotAllowedError : BaseInputValidationError
    {
        public EmptyNotAllowedError(string fieldNameChinese, string fieldName)
        {
            FieldNameChinese = fieldNameChinese;
            FieldName = fieldName;
        }

        public string FieldName { get; }

        public string FieldNameChinese { get; }

        public override int ErrorCodeInt => 1;
        public override string ErrorMessage => $"{FieldNameChinese.UnicodeQuote()}未提供輸入值，或格式不正確！";
    }
}