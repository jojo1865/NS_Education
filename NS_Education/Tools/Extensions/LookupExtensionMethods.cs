using System.Collections.Generic;
using System.Linq;

namespace NS_Education.Tools.Extensions
{
    public static class LookupExtensionMethods
    {
        public static IEnumerable<TValue> GetValueOrDefault<TKey, TValue>(this ILookup<TKey, TValue> lookup, TKey id)
        {
            return lookup.Contains(id) ? lookup[id] : default;
        }
    }
}