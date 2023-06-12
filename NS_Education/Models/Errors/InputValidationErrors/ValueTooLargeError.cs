namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class ValueTooLargeError : BaseInputValidationError
    {
        public ValueTooLargeError(string fieldName, object max)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);
            AddAdditionalValues(ErrorField.Max, max);
        }

        public override int ErrorCodeInt => 9;

        public override string ErrorMessage =>
            $"{GetAdditionalValueFormatted(ErrorField.FieldName)}的值不得大於{GetAdditionalValueFormatted(ErrorField.Max)}！";
    }
}