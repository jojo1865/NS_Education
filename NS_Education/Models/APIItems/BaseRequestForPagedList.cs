using System;
using System.Collections.Generic;
using NS_Education.Variables;

namespace NS_Education.Models.APIItems
{
    /// <summary>
    /// 包含 List 的通用訊息回傳格式。
    /// </summary>
    public abstract class BaseRequestForPagedList : BaseRequestForList
    {
        /// <summary>
        /// 目前所在頁數。全部顯示時，此值為 0。
        /// </summary>
        public int NowPage { get; set; }

        /// <summary>
        /// 每頁顯示筆數。
        /// </summary>
        public int CutPage { get; set; }

        private int CutPageOrDefault => CutPage == 0 ? IoConstants.DefaultCutPage : CutPage;

        /// <summary>
        /// 查詢時，是否將預設排序倒轉。
        /// </summary>
        public bool ReverseOrder { get; set; } = false;

        /// <summary>
        /// 取得是否啟用分頁處理。
        /// </summary>
        public bool IsPagingEnabled => NowPage > 0;

        public IEnumerable<PagedListSorting> Sorting { get; set; } = new List<PagedListSorting>();

        /// <summary>
        /// 依據目前的 NowPage 和 CutPage，回傳 0-based 的查詢 index。
        /// </summary>
        /// <returns>0-based index</returns>
        public int GetStartIndex()
            => IsPagingEnabled ? (NowPage - 1) * CutPageOrDefault : 0;

        /// <summary>
        /// 依據目前的 NowPage
        /// 與 CutPage，回傳應取得多少筆資料。<br/>
        /// 當不啟用分頁功能時，回傳一百筆資料。 
        /// </summary>
        /// <returns>應取得多少筆資料</returns>
        public int GetTakeRowCount()
            => IsPagingEnabled ? CutPageOrDefault : 100;

        public (int skip, int take) CalculateSkipAndTake(int totalRows)
        {
            // 正序
            // 1 2 3 4 5 6 7 8 9 0
            // +---+ +---+ +---+ +

            // 反序
            // 1 2 3 4 5 6 7 8 9 0
            // + +---+ +---+ +---+

            int left, right;

            if (!ReverseOrder)
            {
                // 正序時，照內建算式取值
                left = GetStartIndex();
                right = left + GetTakeRowCount() - 1;
            }
            else
            {
                // 倒序時，從後方算回來，取得 right
                // 統一轉成 0-index 計算
                right = totalRows - 1 - GetStartIndex();
                left = Math.Max(0, right - (GetTakeRowCount() - 1));
            }

            return (left, right - left + 1);
        }
    }
}