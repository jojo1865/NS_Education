using System;

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

        /// <summary>
        /// 驗證數字是否大於 0。
        /// </summary>
        /// <param name="i">整數</param>
        /// <returns>true：大於 0。<br/>
        /// false：小於或等於 0。
        /// </returns>
        public static bool IsAboveZero(this int i)
        {
            return i > 0;
        }

        /// <summary>
        /// 驗證數字是否大於 0。
        /// </summary>
        /// <param name="i">整數</param>
        /// <returns>true：大於 0。<br/>
        /// false：小於或等於 0。
        /// </returns>
        public static bool IsAboveZero(this int? i)
        {
            return i > 0;
        }

        /// <summary>
        /// 驗證數字是否大於或等於 0。
        /// </summary>
        /// <param name="i">整數</param>
        /// <returns>true：大於或等於 0。<br/>
        /// false：小於 0。
        /// </returns>
        public static bool IsZeroOrAbove(this int i)
        {
            return i >= 0;
        }

        /// <summary>
        /// 驗證數字是否大於或等於 0。
        /// </summary>
        /// <param name="i">整數</param>
        /// <returns>true：大於或等於 0。<br/>
        /// false：小於 0。
        /// </returns>
        public static bool IsZeroOrAbove(this int? i)
        {
            return i >= 0;
        }

        /// <summary>
        /// 檢查兩個範圍（各有 2 個整數代表開頭與結尾）是否有重疊。僅起始點或結束點相等時不算。
        /// </summary>
        /// <param name="rangeA">範圍 A</param>
        /// <param name="rangeB">範圍 B</param>
        /// <returns>
        /// true：存在重疊區間<br/>
        /// false：皆無重疊
        /// </returns>
        public static bool IsCrossingWith(this (int start, int end) rangeA, (int start, int end) rangeB)
        {
            // 確定 start, end 大小值都符合預期，沒有就進行調換
            if (rangeA.start > rangeA.end)
                (rangeA.start, rangeA.end) = (rangeA.end, rangeA.start);

            if (rangeB.start > rangeB.end)
                (rangeB.start, rangeB.end) = (rangeB.end, rangeB.start);

            return rangeA.end > rangeB.start && rangeB.end > rangeA.start;
        }

        /// <summary>
        /// 檢查兩個範圍（各有 2 個整數代表開頭與結尾）中，A 是否包含了 B。完全相等時亦回傳 true。
        /// </summary>
        /// <param name="rangeA">範圍 A</param>
        /// <param name="rangeB">範圍 B</param>
        /// <returns>
        /// true：包含<br/>
        /// false：不包含
        /// </returns>
        public static bool IsIncluding(this (int start, int end) rangeA, (int start, int end) rangeB)
        {
            // 確定 start, end 大小值都符合預期，沒有就進行調換
            if (rangeA.start > rangeA.end)
                (rangeA.start, rangeA.end) = (rangeA.end, rangeA.start);

            if (rangeB.start > rangeB.end)
                (rangeB.start, rangeB.end) = (rangeB.end, rangeB.start);

            return rangeA.start <= rangeB.start && rangeB.end <= rangeA.end;
        }

        /// <summary>
        /// 將數字加上營業稅額。
        /// </summary>
        public static int ToTaxIncluded(this int num)
        {
            return (int)Math.Round(num * 1.05m);
        }
    }
}