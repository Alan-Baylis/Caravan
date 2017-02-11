using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Code.BehaviourTree
{
    class StaticReturnNode : BaseNode
    {
        public NodeState GetValue()
        {
            return NodeState.SUCCESS;
        }
    }
}
