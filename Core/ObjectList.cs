using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public class ObjectList : List<Object>
    {
        public ObjectList(IEnumerable<Object> collection) : base(collection) { }
        public ObjectList() { }
        public ObjectList(params Object[] items) : base(items) {}
    }
}
