namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class LengthTooLongError : BaseInputValidationError
    {
        public LengthTooLongError(string fieldName, int maxLength)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);
            AddAdditionalValues(ErrorField.MaxLength, maxLength);
        }

        public override int ErrorCodeInt => 8;

        public override string ErrorMessage =>
            $"{GetAdditionalValueFormatted(ErrorField.FieldName)}長度不得超過{GetAdditionalValueFormatted(ErrorField.MaxLength)}！";
    }
}