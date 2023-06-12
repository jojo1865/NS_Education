namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class ValueOutOfRangeError : BaseInputValidationError
    {
        public ValueOutOfRangeError(string fieldName, object min, object max)
        {
            AddAdditionalValues(ErrorField.FieldName, fieldName);

            if (min != null)
                AddAdditionalValues(ErrorField.Min, min);

            if (max != null)
                AddAdditionalValues(ErrorField.Max, max);
        }

        public override int ErrorCodeInt => 6;
        public override string ErrorMessage => GetMessage();

        private string GetMessage()
        {
            object min = GetAdditionalValueFormatted(ErrorField.Min);
            object max = GetAdditionalValueFormatted(ErrorField.Max);
            return $"{GetAdditionalValueFormatted(ErrorField.FieldName)}的值超出允許範圍"
                   + (min != null ? $"，最小值{min}" : "")
                   + (max != null ? $"，最大值{max}" : "")
                   + "！";
        }
    }
}