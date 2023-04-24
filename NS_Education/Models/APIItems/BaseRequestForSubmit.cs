namespace NS_Education.Models.APIItems
{
    /// <summary>
    /// Submit 使用的通用訊息回傳格式。
    /// </summary>
    public abstract class BaseRequestForSubmit
    {
        /// <summary>
        /// 設定此筆資料的啟用狀態。<br/>
        /// true：啟用<br/>
        /// false：停用
        /// </summary>
        public bool ActiveFlag { get; set; } = true;
    }
}