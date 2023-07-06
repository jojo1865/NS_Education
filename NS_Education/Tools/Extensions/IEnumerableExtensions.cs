using System;
using System.Collections.Generic;
using System.Linq;

namespace NS_Education.Tools.Extensions
{
    public static class IEnumerableExtensions
    {
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
    }
}