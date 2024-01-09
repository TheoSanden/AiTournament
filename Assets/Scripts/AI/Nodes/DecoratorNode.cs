using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes
{
    public abstract class DecoratorNode : Node
    {
        public Node     m_child;

        #region Properties

        #endregion

        protected override void OnStart()
        {
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            return m_child.Update();
        }

        public override Node Clone()
        {
            DecoratorNode clone = base.Clone() as DecoratorNode;
            clone.m_child = m_child.Clone();
            return clone;
        }
    }
}