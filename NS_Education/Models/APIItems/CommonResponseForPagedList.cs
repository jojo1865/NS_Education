using NS_Education.Variables;

namespace NS_Education.Models.APIItems
{
    /// <summary>
    /// 包含 List 的通用訊息回傳格式。其中包含的僅單一分頁資料。
    /// </summary>
    public class CommonResponseForPagedList<T> : CommonResponseForList<T>
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
        /// 查詢結果全部總共幾筆。
        /// </summary>
        public int AllItemCt { get; set; }

        /// <summary>
        /// 查詢結果全部總共幾分頁。 
        /// </summary>
        public int AllPageCt => NowPage == 0 || AllItemCt == 0
            ? 0
            : (AllItemCt / CutPageOrDefault + (AllItemCt % CutPageOrDefault == 0 ? 0 : 1));

        /// <summary>
        /// 將一些來自於輸入資料的值帶入本物件。
        /// </summary>
        /// <param name="input">輸入資料</param>
        public virtual void SetByInput(BaseRequestForPagedList input)
        {
            NowPage = input.NowPage;
            CutPage = input.CutPage;
        }
    }
}