using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes
{
    public class Increment : DecoratorNode
    {
        public string m_key = "VariableName";

        #region Properties

        public override string Description => "Increment " + m_key;

        #endregion

        protected override void OnStart()
        {
            if (Tree != null && Tree.Blackboard != null)
            {
                int iValue = Tree.Blackboard.GetValue(m_key, 0);
                Tree.Blackboard.SetValue(m_key, iValue + 1);
            }
        }
    }
}