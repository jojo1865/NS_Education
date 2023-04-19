using System.Linq;
using System.Threading.Tasks;

namespace NS_Education.Controllers.BaseClass.FunctionInterface
{
    public interface IChangeActive<out TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 修改資料的啟用/停用狀態。
        /// </summary>
        /// <param name="id">欲刪除資料的查詢索引值</param>
        /// <param name="activeFlag">欲修改成的狀態</param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入驗證失敗、查無資料、DB 異常時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他錯誤時：拋錯。
        /// </returns>
        Task<string> ChangeActive(int id, bool? activeFlag);
        
        /// <summary>
        /// 修改啟用/停用狀態的查詢。
        /// </summary>
        /// <param name="id">欲修改狀態資料的 ID</param>
        /// <returns>查詢</returns>
        /// <remarks>若此方法是藉由 Helper 被呼叫時，實作者在查詢中可以忽略 DeleteFlag 的判定。</remarks>
        IQueryable<TEntity> ChangeActiveQuery(int id);
    }
}