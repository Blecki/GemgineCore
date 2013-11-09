using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.StateMachine
{
    public class State
    {
        public List<Transition> transitions = new List<Transition>();
        public Action<Machine> update = null;
        public Action<Machine> onEnter = null;

        public void Update(Machine machine)
        {
            if (update != null) update(machine);
        }

        public Transition Check(Machine machine)
        {
            foreach (var trans in transitions)
                if (trans.condition != null && trans.condition(machine)) return trans;
            return null;
        }
    }
}
