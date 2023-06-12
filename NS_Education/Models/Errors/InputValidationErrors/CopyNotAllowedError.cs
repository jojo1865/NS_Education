namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class CopyNotAllowedError : BaseInputValidationError
    {
        public CopyNotAllowedError(string fieldName, string keyFieldName = null)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);

            if (keyFieldName != null)
                AddAdditionalValues(ErrorField.KeyFieldName, keyFieldName);
        }

        public override int ErrorCodeInt => 10;
        public override string ErrorMessage => GetMessage();

        private string GetMessage()
        {
            string fieldName = GetAdditionalValueFormatted(ErrorField.FieldName);
            string keyFieldName = GetAdditionalValueFormatted(ErrorField.KeyFieldName);

            return keyFieldName != null
                ? $"{fieldName}中的{keyFieldName}不允許重複輸入！"
                : $"{fieldName}不允許重複輸入！";
        }
    }
}