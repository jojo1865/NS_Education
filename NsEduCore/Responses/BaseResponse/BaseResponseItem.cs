namespace NsEduCore.Responses.BaseResponse
{
    /// <summary>
    /// 用於諸如 GetList 等情況時，包含 Items 的回傳用物件。此為 Items 中每項物件的格式。
    /// </summary>
    public class BaseResponseItem
    {
        /// <summary>
        /// 此 Item 的編號。
        /// </summary>
        public int ID { get; set; }
        
        /// <summary>
        /// 此 Item 的名稱。
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 此 Item 的是否被選取狀態。
        /// </summary>
        public bool SelectFlag { get; set; }
    }
}