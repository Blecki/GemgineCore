using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Gem
{
    public class InputActionHandler
    {
        private Dictionary<String, Action<ObjectList>> actionMappings = new Dictionary<string, Action<ObjectList>>();

        public void MapAction(String actionName, Action<ObjectList> handler)
        {
            actionMappings.Upsert(actionName, handler);
        }

        public void MapAction(String actionName, Action handler)
        {
            actionMappings.Upsert(actionName, ActionHelper.WrapAction(handler));
        }

        public void CheckAndFireMappings(Input input, Gem.ComponentModel.Simulation sim)
        {
            foreach (var mapping in actionMappings)
                if (input.Check(mapping.Key)) sim.EnqueueEvent("@raw-input-event", new ObjectList(mapping.Value));
        }

        public List<ObjectList> CheckAndFireMappingsEx(Input input)
        {
            var r = new List<ObjectList>();
            foreach (var mapping in actionMappings)
                if (input.Check(mapping.Key)) r.Add(new ObjectList(mapping.Value));
            return r;
        }

    }
}
