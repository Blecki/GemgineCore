using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Gem
{
    public class InputActionHandler
    {
        private Dictionary<String, Action<Common.ObjectList>> actionMappings = new Dictionary<string, Action<Common.ObjectList>>();

        public void MapAction(String actionName, Action<Common.ObjectList> handler)
        {
            actionMappings.Upsert(actionName, handler);
        }

        public void MapAction(String actionName, Action handler)
        {
            actionMappings.Upsert(actionName, Common.ActionHelper.WrapAction(handler));
        }

        public void CheckAndFireMappings(Input input, Gem.ComponentModel.Simulation sim)
        {
            foreach (var mapping in actionMappings)
                if (input.Check(mapping.Key)) sim.EnqueueEvent("@raw-input-event", new Common.ObjectList(mapping.Value));
        }

        public List<Common.ObjectList> CheckAndFireMappingsEx(Input input)
        {
            var r = new List<Common.ObjectList>();
            foreach (var mapping in actionMappings)
                if (input.Check(mapping.Key)) r.Add(new Common.ObjectList(mapping.Value));
            return r;
        }

    }
}
