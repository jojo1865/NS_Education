using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using NS_Education.Models.APIItems;

namespace NS_Education.Tools.Extensions
{
    public static class IQueryableExtensions
    {
        /// <summary>
        /// 將指定的查詢依據指定的 ListSorting 進行排序。
        /// </summary>
        /// <param name="query">查詢</param>
        /// <param name="sorts">排序方式</param>
        /// <typeparam name="TEntity">資料模型的類型</typeparam>
        /// <returns>
        /// sorts 有值時：排序過的查詢
        /// 沒有值時：原本的查詢
        /// </returns>
        public static IOrderedQueryable<TEntity> SortWithInput<TEntity>(this IQueryable<TEntity> query,
            IEnumerable<ListSorting> sorts)
            where TEntity : class
        {
            sorts = sorts?.ToArray();

            if (sorts is null || !sorts.Any())
                return (IOrderedQueryable<TEntity>)query;

            sorts.ValidateSorts<TEntity>();

            IEnumerable<string> sortStrings =
                sorts.Select(s => s.PropertyName + " " + (s.IsAscending ? "ASC" : "DESC"));

            string sortString = String.Join(", ", sortStrings);

            // Linq.Dynamic
            return query.OrderBy(sortString);
        }
    }
}