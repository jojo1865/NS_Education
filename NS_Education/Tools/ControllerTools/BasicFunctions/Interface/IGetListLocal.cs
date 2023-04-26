using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Interface
{
    /// <summary>
    /// GetList 功能 API 端點的介面。傳入時單純只執行 GET 而無其他參數，不與 DB 互動，回傳所有掌管資料。
    /// </summary>
    /// <typeparam name="TGetListRow">回傳時，List 中子物件的類型</typeparam>
    public interface IGetListLocal<TGetListRow>
        where TGetListRow : class
    {
        /// <summary>
        /// 取得所有資料。
        /// </summary>
        /// <returns>
        /// 成功時：包含列表的通用訊息回傳格式。<br/>
        /// 驗證失敗，或找不到資料時：不包含列表的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        Task<string> GetList();

        /// <summary>
        /// 將取得列表的查詢結果轉換成 Response 所需的子物件類型。。<br/>
        /// 實作者可以在這個方法中進行 AddError，最後回傳結果仍會包含資料，但會告知前端結果並不成功。（Success = false）
        /// </summary>
        /// <param name="entity">單筆查詢結果</param>
        /// <returns>Response 所需類型的單筆資料</returns>
        [NonAction]
        Task<ICollection<TGetListRow>> GetListLocalResults();
    }
}