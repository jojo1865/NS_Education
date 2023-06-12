namespace NS_Education.Models.Errors.DataValidationErrors
{
    public sealed class DataAlreadyExistsError : BaseDataValidationError
    {
        public DataAlreadyExistsError(string fieldName)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);
        }

        public override int ErrorCodeInt => 3;
        public override string ErrorMessage => GetMessage();

        private string GetMessage()
        {
            string fieldName = GetAdditionalValueFormatted(ErrorField.FieldName);
            return fieldName != null ? $"已存在相同{fieldName}的資料！" : "資料已存在！";
        }
    }
}