using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gem.StateMachine
{
    public class Machine
    {
        private State _currentState = null;
        public Object world { get; private set; }
        public Object thisObject { get; private set; }
        public float stateTime { get; private set; }

        public State currentState 
        {
            get { return _currentState; }
            set
            {
                _currentState = value;
                if (_currentState != null && _currentState.onEnter != null)
                    _currentState.onEnter(this);
                stateTime = 0;
            }
        }

        public Machine(Object world, Object thisObject)
        {
            this.world = world;
            this.thisObject = thisObject;
            stateTime = 0;
        }

        public void Update(float elapsedTime)
        {
            stateTime += elapsedTime;
            if (_currentState != null)
            {
                _currentState.Update(this);
                var trans = _currentState.Check(this);
                if (trans != null)
                {
                    currentState = trans.nextState;
                    if (trans.onChange != null) trans.onChange(this);
                }
            }
        }

    }
}
