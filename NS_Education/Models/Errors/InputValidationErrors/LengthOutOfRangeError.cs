namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class LengthOutOfRangeError : BaseInputValidationError
    {
        public LengthOutOfRangeError(string fieldName, int? minLength, int? maxLength)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);

            if (minLength != null)
                AddAdditionalValues(ErrorField.MinLength, minLength.Value);

            if (maxLength != null)
                AddAdditionalValues(ErrorField.MaxLength, maxLength.Value);
        }

        public override int ErrorCodeInt => 3;
        public override string ErrorMessage => GetMessage();

        private string GetMessage()
        {
            string min = GetAdditionalValueFormatted(ErrorField.MinLength);
            string max = GetAdditionalValueFormatted(ErrorField.MaxLength);
            return $"{GetAdditionalValueFormatted(ErrorField.FieldName)}欄位輸入值超出支援的長度"
                   + (min != null ? $"，最小值{min}" : "")
                   + (max != null ? $"，最大值{max}" : "")
                   + "！";
        }
    }
}