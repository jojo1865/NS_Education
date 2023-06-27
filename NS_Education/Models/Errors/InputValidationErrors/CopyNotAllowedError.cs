using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class CopyNotAllowedError : BaseInputValidationError
    {
        public CopyNotAllowedError(string fieldNameChinese, string fieldName)
        {
            FieldNameChinese = fieldNameChinese;
            FieldName = fieldName;
        }

        public CopyNotAllowedError(string fieldNameChinese, string fieldName, string keyFieldNameChinese,
            string keyFieldName)
        {
            FieldNameChinese = fieldNameChinese;
            FieldName = fieldName;
            KeyFieldNameChinese = keyFieldNameChinese;
            KeyFieldName = keyFieldName;
        }

        public string FieldName { get; }

        public string FieldNameChinese { get; }

        public string KeyFieldName { get; }
        public string KeyFieldNameChinese { get; }

        public override int ErrorCodeInt => 10;
        public override string ErrorMessage => GetMessage();

        private string GetMessage()
        {
            return KeyFieldNameChinese != null
                ? $"{FieldNameChinese.UnicodeQuote()}中的{KeyFieldNameChinese.UnicodeQuote()}不允許重複輸入！"
                : $"{FieldNameChinese.UnicodeQuote()}不允許重複輸入！";
        }
    }
}