using Game;
using Graphs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Theo_Sanden
{
    enum UnitStates 
    {
        SEEK,
        SEEKCOVER,
        SETCOVER,
        WAITCOVER
    }
    public class Unit_Theo_Sanden : Unit
    {
        #region Properties

        public new Team_Theo_Sanden Team => base.Team as Team_Theo_Sanden;
        UnitStates currentState = UnitStates.SEEK;
        [SerializeField]
        /*private BehaviourTree tree;
        private Brain brain;*/
        #endregion

        protected override Unit SelectTarget(List<Unit> enemiesInRange)
        {
            float lowestValue = float.MaxValue;
            Unit returnTarget = Team.lamb;

            if (enemiesInRange.Contains(Team.lamb)) 
            {
                returnTarget = Team.lamb;
                if (IsEnemyInCover(Team.lamb,out lowestValue)) 
                {
                    lowestValue /= 2;
                    foreach(Unit unit in enemiesInRange) 
                    {
                        float otherValue;
                        IsEnemyInCover(unit, out otherValue);
                        if(otherValue < lowestValue) 
                        {
                            returnTarget = unit;
                            lowestValue = otherValue;
                        }
                    }
                }
                return returnTarget;
            }

            foreach(Unit unit in enemiesInRange) 
            {
                float otherValue;
                IsEnemyInCover(unit, out otherValue);

                if (otherValue < lowestValue) 
                {
                    lowestValue = otherValue;
                    returnTarget = unit;
                }
            }
            return returnTarget;
        }
        protected override GraphUtils.Path GetPathToTarget()
        {
            if (new List<Unit>(EnemiesInRange).Count > 0) 
            {
                return Astar.GetClosestPath(CurrentNode, TargetNode, new Dictionary<Battlefield.Node, float>[] { BattleFieldAnalysis.Instance.ExposedMap, BattleFieldAnalysis.Instance.WalkableNodes, BattleFieldAnalysis.Instance.FirePowerMap }, new float[] {10,5,1});
            }
            return Astar.GetClosestPath(CurrentNode,TargetNode,new Dictionary<Battlefield.Node, float>[]{BattleFieldAnalysis.Instance.WalkableNodes,BattleFieldAnalysis.Instance.InfluenceMap});
        }
        protected override void Start()
        {
            base.Start();
            StartCoroutine(StupidLogic());
        }
        //low covervalue == good
        private bool IsEnemyInCover(Unit enemy, out float coverValue) 
        {
            if(enemy == null || enemy.CurrentNode == null) { coverValue = 0; return false; }
            if(Battlefield.Instance.HasAnyCoverAt(enemy.CurrentNode)) 
            {
                Vector3 rockPosition = Vector3.zero;
                //This doesnt work that great for multiple rocks
                foreach (Vector2Int dir in BattleFieldAnalysis.cardinalDirections)
                {
                    if (Battlefield.Instance[enemy.CurrentNode.Position + dir] is Node_Rock rock)
                    {
                        rockPosition = rock.WorldPosition;
                        break;
                    }
                }
                if (rockPosition == Vector3.zero) { coverValue = 0; return false; }

                Vector3 nodeToRock = (rockPosition - enemy.CurrentNode.WorldPosition);
                Vector3 nodeToMe = (this.gameObject.transform.position - enemy.CurrentNode.WorldPosition);

                float dot = Vector3.Dot(nodeToRock, nodeToMe);
                coverValue = (1 + dot) / 2;
                if (dot > 0) 
                {
                    return true;
                }
                return false;
            }

            coverValue = 0;
            return false;
        }
        IEnumerator StupidLogic()
        {
            while (true)
            {
                switch (currentState) 
                {

                    case UnitStates.SEEK:
                        //Why is this still throwing excpetions <.<
                        if (Team && Team.lamb && Team.lamb.CurrentNode != null)
                        {
                            /*List<Graphs.ILink> linkList = new List<Graphs.ILink>(Team.lamb.TargetNode.Links);
                            TargetNode = linkList[Random.Range(0, linkList.Count - 1)].Target as Battlefield.Node;
                            */

                            BattleFieldAnalysis.Instance.ClearNode(this);
                            TargetNode = BattleFieldAnalysis.Instance.GetLowestScoreInRange(this,Team.lamb.CurrentNode,9,BattleFieldAnalysis.Instance.FirePowerMap);
                        }
                        else
                        {
                            TargetNode = Battlefield.Instance.GetRandomNode();
                        }

                        if (new List<Unit>(EnemiesInRange).Count > 1)
                        {
                            currentState = UnitStates.SEEKCOVER;
                            TargetNode = BattleFieldAnalysis.Instance.GetBestCoverInRange(this,5);
                        }

                        break;
                    case UnitStates.SEEKCOVER:

                        if(new List<Unit>(EnemiesInRange).Count == 0) 
                        {
                            currentState = UnitStates.SEEK;
                            BattleFieldAnalysis.Instance.ClearNode(this);
                        }

                        if (CurrentNode == TargetNode) 
                        {
                            TargetNode = null;
                            currentState = UnitStates.WAITCOVER;
                            Debug.Log("Setting waitCover");
                        }
                        break;
                    case UnitStates.WAITCOVER:

                        if (new List<Unit>(EnemiesInRange).Count == 0)
                        {
                            currentState = UnitStates.SEEK;
                            BattleFieldAnalysis.Instance.ClearNode(this);
                        }
                        break;
                }
                yield return new WaitForSeconds(1f);
            }
        }
    }
}