using NsEduCore.Requests.BaseRequest;
using NsEduCore.Variables;

namespace NsEduCore.Requests.Company
{
    /// <summary>
    /// Company 取得列表用的要求物件。
    /// </summary>
    public class CompanyGetListRequest : BaseGetListRequestAbstract
    {
        /// <summary>
        /// 搜尋用的關鍵字。（可選）<br/>
        /// 省略時，不列入查詢條件。
        /// </summary>
        public string Keyword { get; set; }
        
        /// <summary>
        /// 公司分類 ID。（可選）<br/>
        /// 省略時，不列入查詢條件。
        /// </summary>
        public int? CompanyTypeId { get; set; }
    }
}