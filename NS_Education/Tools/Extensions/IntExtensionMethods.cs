namespace NS_Education.Tools.Extensions
{
    public static class IntExtensionMethods
    {
        /// <summary>
        /// 檢查數字是否處於兩個整數 min 和 max 之間。<br/>
        /// 若 min 大於 max，會自動交換 min 和 max 再作判定。
        /// </summary>
        /// <param name="i">欲檢查的數字</param>
        /// <param name="min">範圍最小值</param>
        /// <param name="max">範圍最大值</param>
        /// <returns>
        /// true: 數字落於範圍中<br/>
        /// false: 數字位於範圍外
        /// </returns>
        public static bool IsInBetween(this int i, int min, int max)
        {
            if (min > max)
                (min, max) = (max, min);

            return min <= i && i <= max;
        }
    }
}