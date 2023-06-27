using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class WrongFormatError : BaseInputValidationError
    {
        public WrongFormatError()
        {
        }

        public WrongFormatError(string fieldNameChinese, string fieldName)
        {
            FieldNameChinese = fieldNameChinese;
            FieldName = fieldName;
        }

        public string FieldNameChinese { get; }
        public string FieldName { get; }

        public override int ErrorCodeInt => 5;
        public override string ErrorMessage => $"{FieldNameChinese.UnicodeQuote()}格式錯誤！";
    }
}