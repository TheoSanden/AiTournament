using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Node_Rock : Node_Grass
    {

        #region Properties

        public override float UnitMoveSpeed => 0.01f;

        public override Color Color => new Color(0.0f, 0.0f, 1.0f, 0.0f);

        #endregion

        public Node_Rock(Vector2Int vPosition) : base(vPosition)
        {
        }

        public override void CreateLinks()
        {
            // no links from Rock nodes
        }
    }
}
