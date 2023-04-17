using System.Collections.Generic;
using NS_Education.Tools;

namespace NS_Education.Models.APIItems
{
    /// <summary>
    /// 包含 List 的通用訊息回傳格式。
    /// </summary>
    public abstract class BaseRequestForList<T> : cReturnMessageInfusableAbstract
    {
        /// <summary>
        /// 目前所在頁數。全部顯示時，此值為 0。
        /// </summary>
        public int NowPage { get; set; }

        /// <summary>
        /// 每頁顯示筆數。
        /// </summary>
        public int CutPage { get; set; }
        
        /// <summary>
        /// 查詢結果全部總共幾筆。
        /// </summary>
        public int AllItemCt { get; set; }
        
        /// <summary>
        /// 查詢結果全部總共幾分頁。 
        /// </summary>
        public int AllPageCt => CutPage == 0 || AllItemCt == 0 ? 0 : (AllItemCt / CutPage + (AllItemCt % CutPage == 0 ? 0 : 1)); 

        /// <summary>
        /// 實際每行資料內容。
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// 將一些來自於輸入資料的值帶入本物件。
        /// </summary>
        /// <param name="input">輸入資料</param>
        public void SetByInput(BaseRequestForList input)
        {
            NowPage = input.NowPage;
            CutPage = input.CutPage;
        }
    }
}