using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public static class UpsertExtension
    {
        public static void Upsert<K, V>(this Dictionary<K, V> dict, K key, V value)
        {
            if (dict.ContainsKey(key)) dict[key] = value;
            else dict.Add(key, value);
        }
    }
}
