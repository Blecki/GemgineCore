using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public static class UpsertExtension
    {
        /// <summary>
        /// If the dictionary contains key, update the associated value.
        /// Otherwise, insert it.
        /// </summary>
        /// <typeparam name="K"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Upsert<K, V>(this Dictionary<K, V> dict, K key, V value)
        {
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
        }
    }
}
