using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using BeingValidated;
using NS_Education.Models.APIItems;

namespace NS_Education.Tools.Extensions
{
    public static class IQueryableExtensions
    {
        /// <summary>
        /// 將指定的查詢依據指定的 PagedListSorting 進行排序。
        /// </summary>
        /// <param name="query">查詢</param>
        /// <param name="sorts">排序方式</param>
        /// <typeparam name="TEntity">資料模型的類型</typeparam>
        /// <returns>
        /// sorts 有值時：排序過的查詢
        /// 沒有值時：原本的查詢
        /// </returns>
        public static IOrderedQueryable<TEntity> SortWithInput<TEntity>(this IQueryable<TEntity> query,
            IEnumerable<PagedListSorting> sorts)
            where TEntity : class
        {
            sorts = sorts?.ToArray();

            if (sorts is null || !sorts.Any())
                return (IOrderedQueryable<TEntity>)query;

            ValidateSorts<TEntity>(sorts);

            IEnumerable<string> sortStrings =
                sorts.Select(s => s.PropertyName + " " + (s.IsAscending ? "ASC" : "DESC"));

            string sortString = String.Join(", ", sortStrings);

            // Linq.Dynamic
            return query.OrderBy(sortString);
        }

        private static void ValidateSorts<TEntity>(IEnumerable<PagedListSorting> sorts) where TEntity : class
        {
            ICollection<string> errors = new List<string>();

            bool isValid = sorts.StartValidateElements()
                .Validate(
                    sort => sort.PropertyName.HasContent() && typeof(TEntity).GetProperty(sort.PropertyName) != null,
                    sort => errors.Add($"套用排序時，查無此欄位：{sort.PropertyName}"))
                .IsValid();

            if (!isValid)
                throw new Exception(String.Join(",\n", errors));
        }
    }
}