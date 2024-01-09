using Game;
using AI;
using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Theo_Sanden
{
    public class Team_Theo_Sanden : Team
    {
        [SerializeField]
        private Color   m_myFancyColor;
        private BattleFieldAnalysis battleFieldAnalysis;
        public Unit lamb;
        // public BehaviourTree TeamTree;
        #region Properties

        public override Color Color => m_myFancyColor;

        #endregion

        protected override void Start()
        {
            base.Start();
            battleFieldAnalysis = this.gameObject.AddComponent<BattleFieldAnalysis>();
            Time.timeScale = 2;
        }

        private void Update()
        {
            SetLamb();
        }
        void SetLamb() 
        {
            if (!battleFieldAnalysis.Initialized) { return; }
            Unit MostIsolatedUnit = null;
            float currentNodeValue;
            foreach (Unit unit in EnemyTeam.Units) 
            {
                try 
                {
                    currentNodeValue = battleFieldAnalysis.InfluenceMap[unit.CurrentNode];
                }
                catch 
                {
                    continue;
                }
                if (MostIsolatedUnit == null) 
                {
                    MostIsolatedUnit = unit;
                    continue;
                }
                else if(currentNodeValue < battleFieldAnalysis.InfluenceMap[MostIsolatedUnit.CurrentNode]) 
                {
                    MostIsolatedUnit = unit;
                }
            }
            lamb = MostIsolatedUnit;
        }
        private void OnDrawGizmos()
        {
            if (lamb != null) 
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(lamb.gameObject.transform.position + Vector3.up, 0.3f);
            }
        }
    }
}