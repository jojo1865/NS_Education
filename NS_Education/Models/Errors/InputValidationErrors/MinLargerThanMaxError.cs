namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class MinLargerThanMaxError : BaseInputValidationError
    {
        public MinLargerThanMaxError(string fieldNameA, string fieldNameB)
        {
            AddAdditionalValues(ErrorField.FieldNameA, fieldNameA);
            AddAdditionalValues(ErrorField.FieldNameB, fieldNameB);
        }

        public override int ErrorCodeInt => 4;

        public override string ErrorMessage =>
            $"{GetAdditionalValueFormatted(ErrorField.FieldNameA)}必須小於等於{GetAdditionalValueFormatted(ErrorField.FieldNameB)}！";
    }
}