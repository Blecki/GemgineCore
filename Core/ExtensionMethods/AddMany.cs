using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public static class AddManyExtension
    {
        public static void AddMany<V>(this List<V> list, params V[] values)
        {
            list.AddRange(values);
        }
    }
}
