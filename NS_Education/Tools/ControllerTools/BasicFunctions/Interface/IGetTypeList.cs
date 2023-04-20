using System.Linq;
using System.Threading.Tasks;
using NS_Education.Models.APIItems;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Interface
{
    public interface IGetTypeList<TEntity, TGetTypeListResponseRow>
        where TEntity : class
        where TGetTypeListResponseRow : BaseResponseRowForType
    {
        /// <summary>
        /// 取得所有類別的查詢。<br/>
        /// 實作者可以在這個方法中進行 AddError，回到主方法後就不會實際執行查詢，而是提早回傳。<br/>
        /// </summary>
        /// <returns>具備排序的所有類別的查詢。</returns>
        /// <remarks>若此方法是藉由 Helper 被呼叫時，實作者在查詢中可以忽略 DeleteFlag 的判定。</remarks>
        IOrderedQueryable<TEntity> GetTypeListQuery();
        
        /// <summary>
        /// 將查詢出來的類別資料，轉換成回傳使用的子物件類別。
        /// </summary>
        /// <param name="entity">資料</param>
        /// <returns>回傳中 List 的子物件類別</returns>
        Task<TGetTypeListResponseRow> GetTypeListEntityToRow(TEntity entity);
    }
}