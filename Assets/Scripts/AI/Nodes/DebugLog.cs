using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes
{
    public class DebugLog : ActionNode
    {
        public string   m_message;








       #region Properties

        public override string Description => m_message;

        #endregion

        protected override void OnStart()
        {
            Debug.Log($"OnStart{m_message}");
        }

        protected override void OnStop()
        {
            Debug.Log($"OnStop{m_message}");
        }

        protected override State OnUpdate()
        {
            Debug.Log($"OnUpdate{m_message}");
            return State.Success;
        }
    }
}