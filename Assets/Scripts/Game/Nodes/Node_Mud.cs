using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Node_Mud : Node_Grass
    {

        #region Properties

        public override float UnitMoveSpeed => 0.4f;

        public override Color Color => new Color(0.0f, 1.0f, 0.0f, 0.0f);

        public override float AdditionalCost => 3.0f;

        #endregion

        public Node_Mud(Vector2Int vPosition) : base(vPosition)
        {
        }
    }
}
