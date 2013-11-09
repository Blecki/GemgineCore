using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.StateMachine
{
    public class Transition
    {
        public Func<Machine, bool> condition = null;
        public Action<Machine> onChange = null;
        public State nextState = null;
    }
}
