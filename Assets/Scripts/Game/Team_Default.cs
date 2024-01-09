using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Team_Default : Team
    {
        private Color   m_color;

        #region Properties

        public override Color Color
        {
            get
            {
                if (m_color.r == 0 &&
                    m_color.g == 0 &&
                    m_color.b == 0 &&
                    m_color.a == 0)
                {
                    m_color = new Color(Random.value, Random.value, Random.value);
                }

                return m_color;
            }
        }

        #endregion

    }
}
