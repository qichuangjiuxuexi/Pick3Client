using System;
using System.Collections.Generic;

namespace AppBase.Utils
{
    /// <summary>
    /// Linq扩展
    /// </summary>
    public static class LinqUtil
    {
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> callback)
        {
            if (enumerable == null || callback == null) return;
            foreach (var item in enumerable)
            {
                callback.Invoke(item);
            }
        }

        public static V TryGetValue<T, V>(this IDictionary<T, V> dictionary, T key, V defaultValue = default)
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out V value))
            {
                return defaultValue;
            }
            return value;
        }

        public static V TryGetValue<V>(this IList<V> list, int index, V defaultValue = default)
        {
            if (list == null || index < 0 || index >= list.Count)
            {
                return defaultValue;
            }
            return list[index];
        }
    }
}