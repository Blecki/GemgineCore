using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.Common
{
    public static class ActionHelper
    {
        public static Action<ObjectList> WrapAction(Action toWrap)
        {
            return new Action<ObjectList>(l => { toWrap(); });
        }

        public static Action<ObjectList> WrapAction<T>(Action<T> toWrap)
        {
            return new Action<ObjectList>(l => { toWrap.DynamicInvoke(l.ToArray()); });
        }

        public static Action<ObjectList> WrapAction<T1, T2>(Action<T1, T2> toWrap)
        {
            return new Action<ObjectList>(l => { toWrap.DynamicInvoke(l.ToArray()); });
        }

        public static Action<ObjectList> WrapAction<T1, T2, T3>(Action<T1, T2, T3> toWrap)
        {
            return new Action<ObjectList>(l => { toWrap.DynamicInvoke(l.ToArray()); });
        }

        public static Action<ObjectList> WrapAction<T1, T2, T3, T4>(Action<T1, T2, T3, T4> toWrap)
        {
            return new Action<ObjectList>(l => { toWrap.DynamicInvoke(l.ToArray()); });
        }
    }
}
