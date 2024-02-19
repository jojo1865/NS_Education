namespace NS_Education.Models.APIItems.Controller.MonthlyTimeSpans.GetList
{
    /// <summary>
    /// 每月可用時段檔 GetList 的回傳物件中，表示單列資料的子物件。
    /// </summary>
    public class MonthlyTimeSpans_GetList_Output_Row_APIItem : BaseGetResponseRowWithCreUpd
    {
        /// <summary>
        /// 年份
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// 已設定月份
        /// </summary>
        public int SetMonths { get; set; }
    }
}