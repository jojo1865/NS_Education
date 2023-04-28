using System;
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
        /// 依據目前的 NowPage 和 CutPage，回傳 0-based 的查詢 index。
        /// </summary>
        /// <returns>0-based index</returns>
        public int GetStartIndex()
            => (Math.Max(0, NowPage - 1)) * CutPageOrDefault;

        /// <summary>
        /// 依據目前的 NowPage 與 CutPage，回傳應取得多少筆資料。<br/>
        /// 當 NowPage 為 0 時，顯示所有資料。 
        /// </summary>
        /// <returns>應取得多少筆資料</returns>
        public int GetTakeRowCount()
            => NowPage == 0 ? Int32.MaxValue : CutPageOrDefault;
    }
}