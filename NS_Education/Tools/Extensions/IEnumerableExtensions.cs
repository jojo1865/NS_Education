using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using NS_Education.Models.APIItems;

namespace NS_Education.Tools.Extensions
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// 將指定的查詢依據指定的 PagedListSorting 進行排序。
        /// </summary>
        /// <param name="enumeration">列舉</param>
        /// <param name="sorts">排序方式</param>
        /// <typeparam name="T">資料模型的類型</typeparam>
        /// <returns>
        /// sorts 有值時：排序過的列舉
        /// 沒有值時：原本的列舉
        /// </returns>
        public static IEnumerable<T> SortWithInput<T>(this IEnumerable<T> enumeration,
            IEnumerable<ListSorting> sorts)
            where T : class
        {
            sorts = sorts?.ToArray();

            if (sorts is null || !sorts.Any())
                return enumeration;

            sorts.ValidateSorts<T>();

            IEnumerable<string> sortStrings =
                sorts.Select(s => s.PropertyName + " " + (s.IsAscending ? "ASC" : "DESC"));

            string sortString = String.Join(", ", sortStrings);

            // Linq.Dynamic
            return enumeration.AsQueryable().OrderBy(sortString).ToArray();
        }

        /// <inheritdoc cref="SortWithInput{T}(System.Collections.Generic.IEnumerable{T},System.Collections.Generic.IEnumerable{NS_Education.Models.APIItems.ListSorting})"/>
        public static IEnumerable<T> SortWithInput<T>(this IEnumerable<T> enumeration,
            BaseRequestForList request) where T : class
        {
            return enumeration.SortWithInput(request.Sorting);
        }

        public static IOrderedEnumerable<T> AsOrderedEnumerable<T>(this IEnumerable<T> enumerable,
            bool ignoreOriginalOrder = false)
        {
            return !ignoreOriginalOrder && enumerable is IOrderedEnumerable<T> orderedEnumerable
                ? orderedEnumerable
                : enumerable.OrderBy(t => "dummy");
        }

        public static IOrderedEnumerable<TElement> Order<TElement, TKey>(this IOrderedEnumerable<TElement> ordering,
            Func<TElement, TKey> func, bool isAscending)
        {
            return isAscending ? ordering.ThenBy(func) : ordering.ThenByDescending(func);
        }

        public static string StringJoin(this IEnumerable<string> enumerable, string separator)
        {
            return String.Join(separator, enumerable);
        }
    }
}