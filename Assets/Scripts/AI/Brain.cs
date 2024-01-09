using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI
{
    public class Brain : MonoBehaviour
    {
        [SerializeField]
        private BehaviourTree  m_tree;

        #region Properties

        public BehaviourTree Tree => m_tree;

        #endregion

        protected virtual void Start()
        {
            if (m_tree != null)
            {
                m_tree = m_tree.Clone();
                m_tree.StartTree(this);
            }
        }

        protected virtual void Update()
        {
            if (m_tree != null &&
                m_tree.CurrentState == Nodes.Node.State.Running)
            {
                m_tree.Update();
            }
        }
    }
}