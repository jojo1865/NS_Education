using System.Collections.Generic;

namespace NS_Education.Tools.Extensions
{
    public static class DictionaryExtensionMethods
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey id)
        {
            dictionary.TryGetValue(id, out TValue result);
            return result;
        }
    }
}