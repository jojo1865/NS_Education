using System.Collections.Generic;
using NS_Education.Tools;

namespace NS_Education.Models.APIItems
{
    /// <summary>
    /// 包含 List 的通用訊息回傳格式。其中包含的僅單一分頁資料。
    /// </summary>
    public class BaseResponseForList<T> : BaseInfusable
    {
        /// <summary>
        /// 實際每行資料內容。
        /// </summary>
        public ICollection<T> Items { get; set; } = new List<T>();
    }
}