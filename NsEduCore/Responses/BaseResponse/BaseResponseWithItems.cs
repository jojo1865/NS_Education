using System.Collections.Generic;
using NsEduCore.Responses.Company;

namespace NsEduCore.Responses.BaseResponse
{
    /// <summary>
    /// 包含 List 的通用訊息回傳格式。
    /// </summary>
    public class BaseResponseWithItems<T> : BaseResponseAbstract
    {
        /// <summary>
        /// 目前所在頁數。全部顯示時，此值為 0。
        /// </summary>
        public int NowPage { get; set; }
        
        /// <summary>
        /// 每頁顯示筆數。
        /// </summary>
        public int PageRowCount { get; set; }
        
        /// <summary>
        /// 所有資料筆數。
        /// </summary>
        public int AllItemCt { get; set; }

        /// <summary>
        /// 所有分頁數量。全部顯示時，此值為 0。
        /// </summary>
        public int AllPageCt => NowPage == 0 ? 0 : CalculateAllPageCount();

        private int CalculateAllPageCount()
        {
            if (AllItemCt == 0 || PageRowCount == 0)
                return 0;
            
            return AllItemCt / PageRowCount + (AllItemCt % PageRowCount == 0 ? 0 : 1);
        }

        /// <summary>
        /// 此回傳訊息的物件清單。
        /// </summary>
        public IEnumerable<CompanyGetListResponseItem> Items { get; set; }
    }
}