using System.Collections.Generic;

namespace NsEduCore.Responses.BaseResponse
{
    /// <summary>
    /// 包含 List 的通用訊息回傳格式。
    /// </summary>
    public class BaseResponseWithItems : BaseResponseAbstract
    {
        /// <summary>
        /// 此回傳訊息的物件清單。
        /// </summary>
        public List<BaseResponseItem> Items { get; } = new();
    }
}