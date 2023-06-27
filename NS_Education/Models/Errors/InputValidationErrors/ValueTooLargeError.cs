using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class ValueTooLargeError : BaseInputValidationError
    {
        public ValueTooLargeError(string fieldNameChinese, string fieldName, object max = null)
        {
            FieldNameChinese = fieldNameChinese;
            FieldName = fieldName;
            Max = max;
        }

        public string FieldNameChinese { get; }
        public string FieldName { get; }
        public object Max { get; }

        public override int ErrorCodeInt => 9;

        public override string ErrorMessage =>
            Max != null
                ? $"{FieldNameChinese.UnicodeQuote()}的值不得大於{Max}！"
                : $"{FieldNameChinese.UnicodeQuote()}的值過大！";
    }
}