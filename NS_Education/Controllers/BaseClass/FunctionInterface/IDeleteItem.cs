using System.Linq;
using System.Threading.Tasks;

namespace NS_Education.Controllers.BaseClass.FunctionInterface
{
    public interface IDeleteItem<out TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 刪除單筆資料。
        /// </summary>
        /// <param name="id">欲刪除資料的查詢索引值</param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入不正確、查無資料、DB 錯誤時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他異常時：拋錯。
        /// </returns>
        Task<string> DeleteItem(int id);
        
        /// <summary>
        /// 刪除單筆資料的查詢。如果與 GetInfoById 的邏輯相同，可以直接使用 base。
        /// </summary>
        /// <param name="id">欲刪除資料的 ID</param>
        /// <returns>查詢</returns>
        /// <remarks>若此方法是藉由 Helper 被呼叫時，實作者在查詢中可以忽略 DeleteFlag 的判定。</remarks>
        IQueryable<TEntity> DeleteItemQuery(int id);
    }
}