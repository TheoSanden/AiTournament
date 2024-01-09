using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes
{
    public class Root : Node
    {
        public Node     m_child;

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (m_child != null)
            {
                return m_child.Update();
            }

            return State.Success;
        }

        public override Node Clone()
        {
            Root clone = base.Clone() as Root;
            clone.m_child = m_child.Clone();
            return clone;
        }
    }
}