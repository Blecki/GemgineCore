using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public class PropertySet : Dictionary<String, Object>
    {
        public PropertySet() { }

        public PropertySet(PropertySet cloneFrom)
        {
            foreach (var item in cloneFrom)
                Add(item.Key, item.Value);
        }

        public PropertySet(params Object[] args)
        {
            if (args.Length % 2 != 0) throw new InvalidProgramException("Generic Script Object must be initialized with pairs");
            for (int i = 0; i < args.Length; i += 2)
                Add(args[i].ToString(), args[i + 1]);
        }
    }
}
