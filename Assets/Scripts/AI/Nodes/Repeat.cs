using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes
{
    public class Repeat : DecoratorNode
    {
        protected override State OnUpdate()
        {
            // keep updating child
            m_child.Update();
            return State.Running;
        }
    }
}