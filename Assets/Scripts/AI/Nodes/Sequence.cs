using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes
{
    public class Sequence : CompositeNode
    {
        private int m_iCurrentChild = 0;

        protected override void OnStart()
        {
            m_iCurrentChild = 0;
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            if (m_children.Count == 0)
            {
                return State.Success;
            }

            Node node = m_children[m_iCurrentChild];
            switch (node.Update())
            {
                case State.Running:
                    return State.Running;

                case State.Success:
                    m_iCurrentChild++;
                    break;

                case State.Failure:
                    return State.Failure;
            }

            return m_iCurrentChild == m_children.Count ? State.Success : State.Running;
        }
    }
}