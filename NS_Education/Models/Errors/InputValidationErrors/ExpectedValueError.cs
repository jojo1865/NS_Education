namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class ExpectedValueError : BaseInputValidationError
    {
        public ExpectedValueError(string fieldName, object expectedValue)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);
            AddAdditionalValues(ErrorField.ExpectedValue, expectedValue);
        }

        public override int ErrorCodeInt => 7;

        public override string ErrorMessage =>
            $"{GetAdditionalValueFormatted(ErrorField.FieldName)}的值應為{GetAdditionalValueFormatted(ErrorField.ExpectedValue)}！";
    }
}