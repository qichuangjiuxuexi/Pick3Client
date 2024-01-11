using System;
using System.Collections.Generic;

namespace WordGame.Utils
{
    public static class ListTool
    {
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

        public static T Random<T>(this ICollection<T> list)
        {
            if (list == null)
            {
                throw new IndexOutOfRangeException("List needs at least one entry to call Random()");
            }

            int randomId = UnityEngine.Random.Range(0, list.Count);
            int count = 0;
            foreach (T s in list)
            {
                if (count == randomId)
                {
                    return s;
                }
                count++;
            }
            throw new IndexOutOfRangeException("List needs at least one entry to call Random()");
        }
    }
}