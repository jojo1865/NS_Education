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
    public interface IGetListUniqueField<TEntity, TResult>
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
        Task<string> GetList(CommonRequestForUniqueField input);

        /// <summary>
        /// 為查詢進行排序。
        /// </summary>
        /// <returns>排序過的查詢。</returns>
        IOrderedQueryable<TEntity> GetListUniqueFieldsOrderQuery(IQueryable<TEntity> query);

        /// <summary>
        /// 根據關鍵字，對查詢進行篩選。
        /// </summary>
        /// <param name="query">查詢</param>
        /// <param name="keyword">關鍵字</param>
        /// <returns>篩選過的查詢</returns>
        IQueryable<TResult> GetListUniqueFieldsApplyKeywordFilter(IQueryable<TResult> query, string keyword);

        /// <summary>
        /// 將物件變形成查詢結果的表示式。
        /// </summary>
        /// <returns>表示式</returns>
        Expression<Func<TEntity, TResult>> GetListUniqueFieldsQueryExpression();
    }
}