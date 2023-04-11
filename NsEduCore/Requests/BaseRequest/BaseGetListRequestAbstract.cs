using NsEduCore.Variables;
using NsEduCore_Tools.Extensions;

namespace NsEduCore.Requests.BaseRequest
{
    /// <summary>
    /// 多筆式查詢時的傳入訊息的通用基本參數。
    /// </summary>
    public class BaseGetListRequestAbstract
    {
        /// <summary>
        /// 目前所在頁數。為 0 時，顯示所有資料（下拉選單模式）。（可選）
        /// </summary>
        public int? NowPage { get; set; }

        /// <summary>
        /// 每頁資料筆數。（可選）<br/>
        /// 省略時，預設為 10 筆。
        /// </summary>
        public int PageRowCount { get; set; } = QueryConstants.DefaultPageRowCount;

        /// <summary>
        /// 依據目前的 NowPage 和 PageRowCount，回傳 0-based 的查詢 index。
        /// </summary>
        /// <returns>0-based index</returns>
        public int GetStartIndex()
            => NowPage.IsNullOrZeroOrLess()? 0 : (int) (NowPage! - 1) * PageRowCount;
    }
}