namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class NotSupportedValueError : BaseInputValidationError
    {
        public NotSupportedValueError(string fieldName)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);
        }

        public override int ErrorCodeInt => 2;
        public override string ErrorMessage => $"{GetAdditionalValueFormatted(ErrorField.FieldName)}欄位不支援輸入的值！";
    }
}