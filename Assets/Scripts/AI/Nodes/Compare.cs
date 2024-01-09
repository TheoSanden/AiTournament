using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.Nodes
{
    public class Compare : DecoratorNode
    {
        public enum Operator
        {
            Equals, 
            NotEquals,
            GreaterThan,
            LessThan,
        };

        public string           m_key = "VariableName";
        public Operator         m_operator = Operator.LessThan;
        public int              m_iValue = 5;

        static string[]         sm_opCodes = new string[] { " == ", " != ", " > ", " < " };

        #region Properties

        public override string Description => m_key + sm_opCodes[(int)m_operator] + m_iValue.ToString();

        #endregion

        protected override State OnUpdate()
        {
            m_state = State.Failure;
            if (Tree != null && Tree.Blackboard != null)
            {
                int iValue = Tree.Blackboard.GetValue(m_key, 0);

                switch (m_operator)
                {
                    case Operator.Equals:
                        m_state = iValue == m_iValue ? State.Running : State.Failure;
                        break;

                    case Operator.NotEquals:
                        m_state = iValue != m_iValue ? State.Running : State.Failure;
                        break;

                    case Operator.GreaterThan:
                        m_state = iValue > m_iValue ? State.Running : State.Failure;
                        break;

                    case Operator.LessThan:
                        m_state = iValue < m_iValue ? State.Running : State.Failure;
                        break;
                }
            }

            // update child?
            if (m_state == State.Running)
            {
                m_state = m_child.Update();
            }

            return m_state;
        }
    }
}