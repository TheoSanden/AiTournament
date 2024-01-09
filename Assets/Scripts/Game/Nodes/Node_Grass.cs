using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Node_Grass : Battlefield.Node
    {
        static Vector2Int[] sm_directions = new Vector2Int[]
        {
            new Vector2Int(-1, -1),  new Vector2Int(0, -1),   new Vector2Int(1, -1),
            new Vector2Int(-1, 0),                            new Vector2Int(1, 0),
            new Vector2Int(-1, 1),   new Vector2Int(0, 1),    new Vector2Int(1, 1)
        };

        #region Properties

        public override float UnitMoveSpeed => 1.5f;

        public override Color Color => new Color(1.0f, 0.0f, 0.0f, 0.0f);

        #endregion

        public Node_Grass(Vector2Int vPosition) : base(vPosition)
        {
        }

        public override void CreateLinks()
        {
            // normal links
            foreach (Vector2Int vDir in sm_directions)
            {
                Vector2Int vTarget = m_vPosition + vDir;
                Battlefield.Node target = Battlefield.Instance[vTarget];

                if (target != null && target is not Node_Rock)
                {
                    // check access to target
                    bool bGoodLink = true;
                    for (int y = Mathf.Min(m_vPosition.y, vTarget.y); y <= Mathf.Max(m_vPosition.y, vTarget.y) && bGoodLink; y++)
                    {
                        for (int x = Mathf.Min(m_vPosition.x, vTarget.x); x <= Mathf.Max(m_vPosition.x, vTarget.x) && bGoodLink; x++)
                        {
                            Battlefield.Node node = Battlefield.Instance[x, y];
                            if (node == null)
                            {
                                bGoodLink = false;
                                break;
                            }
                        }
                    }

                    if (bGoodLink)
                    {
                        m_links.Add(new Link_Normal(this, target));
                    }
                }
            }
        }
    }
}
