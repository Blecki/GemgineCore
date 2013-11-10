using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem
{
    public class ObjectClickHandler
    {
        private Dictionary<UInt32, Action<Common.ObjectList>> handlers = new Dictionary<uint, Action<Common.ObjectList>>();

        public void AddHandler(UInt32 id, Action<Common.ObjectList> handler)
        {
            handlers.Upsert(id, handler);
        }

        public void AddBasicHandler(UInt32 id, Action<UInt32> handler)
        {
            handlers.Upsert(id, Common.ActionHelper.WrapAction(handler));
        }

        public void FireMapping(UInt32 id, Gem.ComponentModel.Simulation sim)
        {
            if (handlers.ContainsKey(id))
                sim.EnqueueEvent("@raw-input-event", new Common.ObjectList(handlers[id], id));
        }
    }
}
