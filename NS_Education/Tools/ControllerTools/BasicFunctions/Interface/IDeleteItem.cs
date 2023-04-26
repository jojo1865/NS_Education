using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Interface
{
    /// <summary>
    /// DeleteItem 功能 API 端點的介面。
    /// </summary>
    /// <typeparam name="TEntity">掌管資料的類型</typeparam>
    public interface IDeleteItem<out TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 刪除單筆資料。
        /// </summary>
        /// <param name="id">欲刪除資料的查詢索引值</param>
        /// <param name="deleteFlag">欲設定的刪除狀態新值</param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入不正確、查無資料、DB 錯誤時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他異常時：拋錯。
        /// </returns>
        Task<string> DeleteItem(int id, bool? deleteFlag);
        
        /// <summary>
        /// 刪除單筆資料的查詢。如果與 GetInfoById 的邏輯相同，可以直接使用 base。
        /// </summary>
        /// <param name="id">欲刪除資料的 ID</param>
        /// <returns>查詢</returns>
        [NonAction]
        IQueryable<TEntity> DeleteItemQuery(int id);
    }
}