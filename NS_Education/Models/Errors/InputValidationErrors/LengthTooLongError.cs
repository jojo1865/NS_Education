namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class LengthTooLongError : BaseInputValidationError
    {
        public LengthTooLongError(string fieldName, int? maxLength)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);

            if (maxLength != null)
                AddAdditionalValues(ErrorField.MaxLength, maxLength.Value);
        }

        public override int ErrorCodeInt => 8;

        public override string ErrorMessage =>
            GetMessage();

        private string GetMessage()
        {
            string fieldName = GetAdditionalValueFormatted(ErrorField.FieldName);
            string maxLength = GetAdditionalValueFormatted(ErrorField.MaxLength);

            return maxLength != null ? $"{fieldName}長度不得超過{maxLength}！" : $"{fieldName}長度過長！";
        }
    }
}