namespace NS_Education.Models.Errors.DataValidationErrors
{
    public sealed class DataNotFoundError : BaseDataValidationError
    {
        public DataNotFoundError(string fieldName)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);
        }

        public override int ErrorCodeInt => 1;
        public override string ErrorMessage => GetMessage();

        private string GetMessage()
        {
            string fieldName = GetAdditionalValueFormatted(ErrorField.FieldName);
            return (fieldName ?? "") + "查無資料！";
        }
    }
}