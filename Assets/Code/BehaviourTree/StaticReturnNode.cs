using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BehaviourTree
{
    class StaticReturnNode : BaseNode
    {
        override public NodeState Tick()
        {
            return NodeState.SUCCESS;
        }
    }
}
