using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Analysis
{
    public class CoverScore : BattlefieldAnalysis
    {
        #region Properties

        protected override float UpdateInterval => 0.5f;

        #endregion

        protected override float CalculateScore(Battlefield.Node node, out bool bShow)
        {
            

            bShow = true;
            return 0.0f;
        }
    }
}