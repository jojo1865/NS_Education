namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class EmptyNotAllowedError : BaseInputValidationError
    {
        public EmptyNotAllowedError(string fieldName)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);
        }

        public override int ErrorCodeInt => 1;
        public override string ErrorMessage => $"{GetAdditionalValueFormatted(ErrorField.FieldName)}未提供輸入值，或格式不正確！";
    }
}