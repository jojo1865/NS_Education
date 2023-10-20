using QuestPDF.Fluent;

namespace NS_Education.Models.Utilities
{
    public static class PrintReportExtensions
    {
        public static void MakeRow(this TableDescriptor td, params string[] cells)
        {
            foreach (string cell in cells)
            {
                td.Cell().Text(cell);
            }
        }

        public static void DrawLine(this TableDescriptor td, int lineThickness, int cellCount)
        {
            for (int i = 0; i < cellCount; i++)
            {
                td.Cell().AlignBottom().PaddingVertical(4f).LineHorizontal(lineThickness);
            }
        }
    }
}