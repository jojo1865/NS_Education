using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class LengthTooLongError : BaseInputValidationError
    {
        public LengthTooLongError(string fieldName, string fieldNameChinese, int? maxLength)
        {
            FieldName = fieldName;
            FieldNameChinese = fieldNameChinese;
            MaxLength = maxLength;
        }

        public string FieldName { get; }
        public string FieldNameChinese { get; }
        public int? MaxLength { get; }

        public override int ErrorCodeInt => 8;

        public override string ErrorMessage =>
            GetMessage();

        private string GetMessage()
        {
            return MaxLength != null
                ? $"{FieldNameChinese.UnicodeQuote()}長度不得超過 {MaxLength}！"
                : $"{FieldNameChinese.UnicodeQuote()}長度過長！";
        }
    }
}