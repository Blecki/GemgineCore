using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public static class AddManyExtension
    {
        /// <summary>
        /// Add a range of values to a list. Eliminates need to create temporary array to pass to AddRange.
        /// </summary>
        /// <typeparam name="V"></typeparam>
        /// <param name="list"></param>
        /// <param name="values"></param>
        public static void AddMany<V>(this List<V> list, params V[] values)
        {
            list.AddRange(values);
        }
    }
}
