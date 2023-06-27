using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class NotSupportedValueError : BaseInputValidationError
    {
        public NotSupportedValueError(string fieldNameChinese, string fieldName, string reason = null)
        {
            FieldName = fieldName;
            FieldNameChinese = fieldNameChinese;
            Reason = reason;
        }

        public string FieldName { get; }
        public string FieldNameChinese { get; }
        public string Reason { get; }
        public override int ErrorCodeInt => 2;
        public override string ErrorMessage => GetErrorMessage();

        private string GetErrorMessage()
        {
            return Reason.HasContent()
                ? $"{FieldNameChinese.UnicodeQuote()}欄位不支援輸入的值！原因：{Reason.UnicodeQuote()}"
                : $"{FieldNameChinese.UnicodeQuote()}欄位不支援輸入的值！";
        }
    }
}