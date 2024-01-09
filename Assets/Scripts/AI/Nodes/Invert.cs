using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

namespace AI.Nodes
{
    public class Invert : DecoratorNode
    {
        protected override State OnUpdate()
        {
            // update child and invert result
            switch (m_child.Update())
            {
                case State.Success:
                    return State.Failure;

                case State.Failure:
                    return State.Success;

                default:
                    return State.Running;
            }
        }
    }
}