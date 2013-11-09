using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.StateMachine
{
    public class Common
    {
        public static Func<Machine, bool> And(Func<Machine, bool> A, Func<Machine, bool> B)
        {
            return (m) => { return A(m) && B(m); };
        }

        public static Func<Machine, bool> Or(Func<Machine, bool> A, Func<Machine, bool> B)
        {
            return (m) => { return A(m) || B(m); };
        }

        public static Func<Machine, bool> Not(Func<Machine, bool> A)
        {
            return (m) => { return !A(m); };
        }

        public static Func<Machine, bool> Time(float time)
        {
            return (m) => { return m.stateTime >= time; };
        }
    }
}
