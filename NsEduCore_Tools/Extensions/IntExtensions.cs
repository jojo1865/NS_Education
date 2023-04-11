namespace NsEduCore_Tools.Extensions
{
    public static class IntExtensions
    {
        public static bool IsNullOrZeroOrLess(this int? i)
            => i is null or <= 0;
    }
}