using System.Linq;
using System.Threading.Tasks;
using NS_Education.Tools;

namespace NS_Education.Controllers.BaseClass.FunctionInterface
{
    public interface IGetInfoById<TEntity, out TGetResponse>
        where TEntity : class
        where TGetResponse : cReturnMessageInfusableAbstract
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
        /// 取得單筆資料時的查詢。
        /// </summary>
        /// <param name="id">使用者輸入的查詢用索引鍵</param>
        /// <returns>查詢。</returns>
        /// <remarks>若此方法是藉由 Helper 被呼叫時，實作者在查詢中可以忽略 DeleteFlag 的判定。</remarks>
        IQueryable<TEntity> GetInfoByIdQuery(int id);

        /// <summary>
        /// 將取得單筆資料時的查詢結果轉換成 Response 所需類型的物件。
        /// </summary>
        /// <param name="entity">原查詢結果</param>
        /// <returns>Response 所需類型的物件</returns>
        TGetResponse GetInfoByIdConvertEntityToResponse(TEntity entity);
    }
}