using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class ValueOutOfRangeError : BaseInputValidationError
    {
        public ValueOutOfRangeError(string fieldNameChinese, string fieldName, object min, object max)
        {
            FieldNameChinese = fieldNameChinese;
            FieldName = fieldName;
            Min = min;
            Max = max;
        }

        public string FieldNameChinese { get; }
        public string FieldName { get; }
        public object Min { get; }
        public object Max { get; }

        public override int ErrorCodeInt => 6;
        public override string ErrorMessage => GetMessage();

        private string GetMessage()
        {
            return $"{FieldNameChinese.UnicodeQuote()}的值超出允許範圍"
                   + (Min != null ? $"，最小值 {Min}" : "")
                   + (Max != null ? $"，最大值 {Max}" : "")
                   + "！";
        }
    }
}