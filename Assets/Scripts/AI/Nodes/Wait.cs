using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes
{
    public class Wait : ActionNode
    {
        public float        m_fTime = 1.0f;

        private float       m_fElapsedTime = 0.0f;


        #region Properties

        private float RemainingTime => m_bStarted && m_state == State.Running ? m_fTime - m_fElapsedTime : 0.0f;

        public override string Description => RemainingTime.ToString("0.00") + " sec";

        #endregion

        protected override void OnStart()
        {            
            m_fElapsedTime = 0.0f;
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            m_fElapsedTime += Time.deltaTime;
            return RemainingTime < 0.0f ? State.Success : State.Running;
        }
    }
}