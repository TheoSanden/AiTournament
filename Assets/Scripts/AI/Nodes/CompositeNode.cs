using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes
{
    public abstract class CompositeNode : Node
    {
        public List<Node> m_children = new List<Node>();

        #region Properties

        #endregion

        public override Node Clone()
        {
            CompositeNode clone = base.Clone() as CompositeNode;
            clone.m_children = m_children.ConvertAll(c => c.Clone());
            return clone;
        }

        public void SortChildren()
        {
            m_children.Sort((Node A, Node B) => { return A.m_vPosition.x < B.m_vPosition.x ? -1 : 1; });
        }
    }
}