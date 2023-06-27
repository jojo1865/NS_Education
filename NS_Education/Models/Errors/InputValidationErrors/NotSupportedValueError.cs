namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class NotSupportedValueError : BaseInputValidationError
    {
        public NotSupportedValueError(string fieldName, string reason = null)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);

            if (reason != null)
                AddAdditionalValues(ErrorField.Reason, reason);
        }

        public override int ErrorCodeInt => 2;
        public override string ErrorMessage => GetErrorMessage();

        private string GetErrorMessage()
        {
            if (GetAdditionalValue(ErrorField.Reason) != null)
                return
                    $"{GetAdditionalValueFormatted(ErrorField.FieldName)}欄位不支援輸入的值！原因：{GetAdditionalValueFormatted(ErrorField.Reason)}";
            else
                return $"{GetAdditionalValueFormatted(ErrorField.FieldName)}欄位不支援輸入的值！";
        }
    }
}