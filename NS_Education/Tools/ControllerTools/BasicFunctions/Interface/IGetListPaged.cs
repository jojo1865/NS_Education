using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Interface
{
    /// <summary>
    /// GetList 功能 API 端點的介面。提供分頁過的回傳。
    /// </summary>
    /// <typeparam name="TEntity">掌管資料的類型</typeparam>
    /// <typeparam name="TGetListPagedRequest">傳入要求的類型</typeparam>
    /// <typeparam name="TGetListPagedRow">回傳時，List 中子物件的類型</typeparam>
    public interface IGetListPaged<TEntity, in TGetListPagedRequest, TGetListPagedRow>
        where TEntity : class
        where TGetListPagedRequest : BaseRequestForList
        where TGetListPagedRow : IGetResponse
    {
        /// <summary>
        /// 取得分頁過的列表。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// 成功時：包含列表的通用訊息回傳格式。<br/>
        /// 驗證失敗時：不包含列表的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        Task<string> GetList(TGetListPagedRequest input);
        
        /// <summary>
        /// 驗證取得列表的輸入資料。<br/>
        /// 當此方法回傳 false 時，回到主方法後就會提早回傳。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// true：驗證通過。<br/>
        /// false：驗證不通過。
        /// </returns>
        [NonAction]
        Task<bool> GetListPagedValidateInput(TGetListPagedRequest input);

        /// <summary>
        /// 依據取得列表的輸入資料，取得查詢。<br/>
        /// 實作者可以在這個方法中進行 AddError，回到主方法後就不會實際執行查詢，而是提早回傳。<br/>
        /// </summary>
        /// <returns>具備排序的查詢。</returns>
        /// <remarks>若此方法是藉由 Helper 被呼叫時，實作者在查詢中應忽略 ActiveFlag 和 DeleteFlag 的判定。</remarks>
        [NonAction]
        IOrderedQueryable<TEntity> GetListPagedOrderedQuery(TGetListPagedRequest input);

        /// <summary>
        /// 將取得列表的查詢結果轉換成 Response 所需的子物件類型。。<br/>
        /// 時作者可以在這個方法中進行 AddError，最後回傳結果仍會包含資料，但會告知前端結果並不成功。（Success = false）
        /// </summary>
        /// <param name="entity">單筆查詢結果</param>
        /// <returns>Response 所需類型的單筆資料</returns>
        [NonAction]
        Task<TGetListPagedRow> GetListPagedEntityToRow(TEntity entity);
    }
}