namespace NS_Education.Models.APIItems.Controller.Resver.GetHistory
{
    public class Resver_GetHistory_Output_Row_APIItem
    {
        /// <summary>
        /// 操作紀錄種類的中文名稱
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 操作時間
        /// </summary>
        public string CreDate { get; set; }

        /// <summary>
        /// 建立者ID 建立者名稱
        /// </summary>
        public string CreUser { get; set; }
    }
}