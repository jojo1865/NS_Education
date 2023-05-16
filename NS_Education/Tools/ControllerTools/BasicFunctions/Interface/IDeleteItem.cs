using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using NS_Education.Models.APIItems.Common.DeleteItem;

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
        /// 刪除單筆或多筆資料。
        /// </summary>
        /// <param name="input">使用者輸入。參照：<see cref="DeleteItem_Input_APIItem"/></param>
        /// <returns>
        /// 成功時：通用訊息回傳格式。<br/>
        /// 輸入不正確、查無資料、DB 錯誤時：包含錯誤訊息的通用訊息回傳格式。<br/>
        /// 其他異常時：拋錯。
        /// </returns>
        Task<string> DeleteItem(DeleteItem_Input_APIItem input);
        
        /// <summary>
        /// 刪除單筆或多筆資料的查詢。
        /// </summary>
        /// <param name="ids">欲刪除資料的 ID</param>
        /// <returns>查詢</returns>
        [NonAction]
        IQueryable<TEntity> DeleteItemsQuery(IEnumerable<int> ids);
    }
}