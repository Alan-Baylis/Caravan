using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourTree
{
    public enum NodeState {
        SUCCESS,
        FAIL,
        RUNNING,
        ERROR
    }

    public abstract class BaseNode
    {
        System.Guid UUID;
        public BaseNode()
        {
            UUID = System.Guid.NewGuid();
        }

        abstract public NodeState Tick();
    }
}
