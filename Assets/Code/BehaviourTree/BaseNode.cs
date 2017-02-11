using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

enum NodeState {
    SUCCESS,
    FAIL,
    RUNNING
}

namespace Assets.Code.BehaviourTree
{
    interface BaseNode
    {
        NodeState GetValue();
    }
}
