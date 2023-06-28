using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using NS_Education.Models.APIItems;

namespace NS_Education.Tools.ControllerTools.BasicFunctions.Interface
{
    /// <summary>
    /// GetUniqueNames 功能 API 端點的介面。提供無分頁的回傳。
    /// </summary>
    /// <typeparam name="TEntity">資料類型</typeparam>
    public interface IGetListUniqueNames<TEntity>
        where TEntity : class
    {
        /// <summary>
        /// 取得無分頁的整批獨特名稱列表。
        /// </summary>
        /// <param name="input">輸入資料</param>
        /// <returns>
        /// 成功時：包含列表的通用訊息回傳格式。<br/>
        /// 驗證失敗時：不包含列表的通用訊息回傳格式。<br/>
        /// 意外錯誤時：拋錯。
        /// </returns>
        Task<string> GetList(CommonRequestForUniqueNames input);

        /// <summary>
        /// 為查詢進行排序。
        /// </summary>
        /// <returns>排序過的查詢。</returns>
        IOrderedQueryable<TEntity> GetListUniqueNamesOrderQuery(IQueryable<TEntity> query);

        /// <summary>
        /// 從物件中取得名稱欄位的表示式。
        /// </summary>
        /// <returns>表示式</returns>
        Expression<Func<TEntity, string>> GetListUniqueNamesQueryExpression();
    }
}