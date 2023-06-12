namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class WrongFormatError : BaseInputValidationError
    {
        public WrongFormatError(string fieldName)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);
        }

        public override int ErrorCodeInt => 5;
        public override string ErrorMessage => $"{GetAdditionalValueFormatted(ErrorField.FieldName)}格式錯誤！";
    }
}