using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class ExpectedValueError : BaseInputValidationError
    {
        public ExpectedValueError(string fieldNameChinese, string fieldName, object expectedValue)
        {
            FieldNameChinese = fieldNameChinese;
            FieldName = fieldName;
            ExpectedValue = expectedValue;
        }

        public string FieldName { get; }

        public string FieldNameChinese { get; }

        public object ExpectedValue { get; }
        public override int ErrorCodeInt => 7;

        public override string ErrorMessage =>
            $"{FieldNameChinese.UnicodeQuote()}的值應為{ExpectedValue.ToString().UnicodeQuote()}！";
    }
}