using NS_Education.Tools.Extensions;

namespace NS_Education.Models.Errors.InputValidationErrors
{
    public sealed class MinLargerThanMaxError : BaseInputValidationError
    {
        public MinLargerThanMaxError(string fieldNameChineseA, string fieldNameA, string fieldNameChineseB,
            string fieldNameB)
        {
            FieldNameA = fieldNameA;
            FieldNameChineseA = fieldNameChineseA;
            FieldNameB = fieldNameB;
            FieldNameChineseB = fieldNameChineseB;
        }

        public string FieldNameA { get; }
        public string FieldNameChineseA { get; }
        public string FieldNameB { get; }
        public string FieldNameChineseB { get; }

        public override int ErrorCodeInt => 4;

        public override string ErrorMessage =>
            $"{FieldNameChineseA.UnicodeQuote()}必須小於等於{FieldNameChineseB.UnicodeQuote()}！";
    }
}