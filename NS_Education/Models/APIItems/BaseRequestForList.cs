using System.Collections.Generic;
using System.Linq;

namespace NS_Education.Models.APIItems
{
    public abstract class BaseRequestForList
    {
        /// <summary>
        /// 依據啟用狀態篩選資料。<br/>
        /// 0：僅篩選停用中資料<br/>
        /// 1：僅篩選啟用中資料<br/>
        /// -1：（預設）不做篩選。
        /// </summary>
        public int ActiveFlag { get; set; } = -1;

        /// <summary>
        /// 依據刪除狀態篩選資料。<br/>
        /// -1：全部顯示。只在管理員時有效。
        /// 0：（預設）僅篩選未刪除<br/>
        /// 1：僅篩選已刪除<br/>
        /// </summary>
        public int DeleteFlag { get; set; } = 0;

        public ICollection<ListSorting> Sorting
        {
            // 消毒，前端丟進來的格式有可能造成誤認為長度為 1 的集合，只包含一個 null
            get => _Sorting ?? new List<ListSorting>();
            set => _Sorting = value.Where(s => s != null).ToList();
        }

        public ICollection<ListSorting> _Sorting { get; set; }
    }
}