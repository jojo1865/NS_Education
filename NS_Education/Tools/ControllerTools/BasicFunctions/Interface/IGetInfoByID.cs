using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Interface
{
    /// <summary>
    /// GetInfoByID 功能 API 端點的介面。
    /// </summary>
    /// <typeparam name="TEntity">掌管資料的類型</typeparam>
    /// <typeparam name="TGetResponse">回傳類型</typeparam>
    public interface IGetInfoById<TEntity, TGetResponse>
        where TEntity : class
        where TGetResponse : IReturnMessageInfusable
    {
        /// <summary>
        /// 取得單筆資料。
        /// </summary>
        /// <param name="id">查詢用的索引鍵</param>
        /// <returns>
        /// 成功時：包含資料的通用訊息回傳格式。<br/>
        /// 輸入驗證失敗，或查無資料時：不包含資料的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        Task<string> GetInfoById(int id);

        /// <summary>
        /// 驗證此次要求的輸入是否符合條件。
        /// </summary>
        /// <param name="id">輸入</param>
        /// <returns>
        /// true：驗證通過<br/>
        /// false：驗證失敗
        /// </returns>
        [NonAction]
        Task<bool> GetInfoByIdValidateInput(int id);
        
        /// <summary>
        /// 取得單筆資料時的查詢。
        /// </summary>
        /// <param name="id">使用者輸入的查詢用索引鍵</param>
        /// <returns>查詢。</returns>
        /// <remarks>若此方法是藉由 Helper 被呼叫時，實作者在查詢中可以忽略 DeleteFlag 的判定。</remarks>
        [NonAction]
        IQueryable<TEntity> GetInfoByIdQuery(int id);

        /// <summary>
        /// 將取得單筆資料時的查詢結果轉換成 Response 所需類型的物件。
        /// </summary>
        /// <param name="entity">原查詢結果</param>
        /// <returns>Response 所需類型的物件</returns>
        [NonAction]
        Task<TGetResponse> GetInfoByIdConvertEntityToResponse(TEntity entity);
    }
}