using System.Collections.Generic;

namespace NsEduCore.Responses.BaseResponse
{
    /// <summary>
    /// 包含 List 的通用訊息回傳格式。
    /// </summary>
    public class BaseResponseWithItems : BaseResponseAbstract
    {
        public List<BaseResponseItem> Items { get; } = new();
    }
}