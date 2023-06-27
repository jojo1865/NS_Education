using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class LengthOutOfRangeError : BaseInputValidationError
    {
        public LengthOutOfRangeError(string fieldNameChinese, string fieldName, int? minLength, int? maxLength)
        {
            FieldNameChinese = fieldNameChinese;
            FieldName = fieldName;
            MinLength = minLength;
            MaxLength = maxLength;
        }

        public string FieldName { get; }
        public string FieldNameChinese { get; }
        public int? MinLength { get; }
        public int? MaxLength { get; }

        public override int ErrorCodeInt => 3;
        public override string ErrorMessage => GetMessage();

        private string GetMessage()
        {
            return $"{FieldNameChinese.UnicodeQuote()}欄位輸入值超出支援的長度"
                   + (MinLength != null ? $"，最小值 {MinLength}" : "")
                   + (MaxLength != null ? $"，最大值 {MaxLength}" : "")
                   + "！";
        }
    }
}