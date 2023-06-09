using System;
using NS_Education.Models.APIItems;

namespace NS_Education.Tools.Extensions
{
    internal static class BaseRequestForPagedListExtension
    {
        public static (int skip, int take) CalculateSkipAndTake(this BaseRequestForPagedList input, int totalRows)
        {
            // 正序
            // 1 2 3 4 5 6 7 8 9 0
            // +---+ +---+ +---+ +

            // 反序
            // 1 2 3 4 5 6 7 8 9 0
            // + +---+ +---+ +---+

            int left, right;

            if (!input.ReverseOrder)
            {
                // 正序時，照內建算式取值
                left = input.GetStartIndex();
                right = left + input.GetTakeRowCount() - 1;
            }
            else
            {
                // 倒序時，從後方算回來，取得 right
                // 統一轉成 0-index 計算
                right = totalRows - 1 - input.GetStartIndex();
                left = Math.Max(0, right - (input.GetTakeRowCount() - 1));
            }

            return (left, right - left + 1);
        }
    }
}