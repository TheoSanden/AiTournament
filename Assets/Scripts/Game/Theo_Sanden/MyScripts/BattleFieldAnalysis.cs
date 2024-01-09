using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine.Profiling;
using UnityEngine;
using Game;

namespace Theo_Sanden
{
    public class BfNode
    {
        public BfNode(Battlefield.Node node, float score)
        {
            this.node = node;
            this.score = score;

        }
        public Battlefield.Node node;
        public float score;
    }
    public class BattleFieldAnalysis : MonoBehaviour
    {
        public Dictionary<Unit, Battlefield.Node> ActiveNodes = new Dictionary<Unit, Battlefield.Node>();

        public static BattleFieldAnalysis Instance;
        public bool Initialized = false;
        Team_Theo_Sanden friendlyTeam;
        float MaxTeamUnitCount = 5;
        public static Vector2Int[] cardinalDirections = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        float mapGenerationFrequency = 1.0f;

        private HashSet<Battlefield.Node> coverNodes = new HashSet<Battlefield.Node>();
        private Dictionary<Battlefield.Node, float> walkableNodes = new Dictionary<Battlefield.Node, float>();
        public Dictionary<Battlefield.Node, float> WalkableNodes
        {
            get => walkableNodes;
        }


        float exposedRange = 3;
        private Dictionary<Battlefield.Node, float> exposedMap = new Dictionary<Battlefield.Node, float>();

        public Dictionary<Battlefield.Node, float> ExposedMap
        {
            get => exposedMap;
        }


        private Dictionary<Battlefield.Node, float> influenceMap = new Dictionary<Battlefield.Node, float>();
        public bool InfluenceMapHasBeenGenerated = false;
        public Dictionary<Battlefield.Node, float> InfluenceMap
        {
            get
            {
                if (!InfluenceMapHasBeenGenerated)
                {
                    GenerateInfluenceMap();
                    InfluenceMapHasBeenGenerated = true;
                    StartCoroutine(CoroutineHelper.SetAfterSeconds(result => InfluenceMapHasBeenGenerated = result, false, mapGenerationFrequency));
                }
                return influenceMap;
            }
        }

        private HashSet<BfNode> coverMap = new HashSet<BfNode>();
        public bool CoverMapHasBeenGenerated = false;
        public HashSet<BfNode> CoverMap
        {
            get
            {
                if (!CoverMapHasBeenGenerated)
                {
                    GenerateCoverMap();
                    CoverMapHasBeenGenerated = true;
                    StartCoroutine(CoroutineHelper.SetAfterSeconds(result => CoverMapHasBeenGenerated = result, false, mapGenerationFrequency));
                }
                return coverMap;
            }
        }

        private Dictionary<Battlefield.Node, float> firePowerMap = new Dictionary<Battlefield.Node, float>();
        public bool FirePowerMapHasBeenCreated = false;

        public Dictionary<Battlefield.Node, float> FirePowerMap
        {
            get
            {
                if (!FirePowerMapHasBeenCreated)
                {
                    GenerateFirePowerMap();
                    FirePowerMapHasBeenCreated = true;
                    StartCoroutine(CoroutineHelper.SetAfterSeconds(result => FirePowerMapHasBeenCreated = result, false, mapGenerationFrequency));
                }
                return firePowerMap;
            }
        }
        private void Start()
        {
            StartCoroutine(LateStart());
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogWarning("Only one battlefield analysis can be online at a time");
                Destroy(this);
            }

        }
        IEnumerator LateStart()
        {
            yield return new WaitForSeconds(1.0f);          
            GenerateWalkableNodes();
            GenerateExposedMap();
            friendlyTeam = FindObjectOfType<Team_Theo_Sanden>();
            Initialized = true;
        }
        private void GenerateWalkableNodes()
        {
            walkableNodes.Clear();
            float MudScore = 7f;
            float GrassScore = 0.0f;
            foreach (Battlefield.Node node in Battlefield.Instance.Nodes)
            {
                if (node as Node_Mud != null)
                {
                    walkableNodes.Add(node, MudScore);
                }
                else if (node as Node_Grass != null)
                {
                    walkableNodes.Add(node, GrassScore);
                }
            }
        }
        private void GenerateInfluenceMap()
        {
            if (!Initialized) return;
            influenceMap.Clear();
            foreach (Battlefield.Node node in Battlefield.Instance.Nodes)
            {
                if (node as Node_Rock != null) { return; }
                float score = 0;
                List<Unit> allUnits = new List<Unit>(friendlyTeam.Units);
                allUnits.AddRange(friendlyTeam.EnemyTeam.Units);
                foreach (Unit unit in allUnits)
                {
                    float distanceToUnit = Vector3.Distance(node.WorldPosition, unit.transform.position);
                    if (distanceToUnit <= Unit.FIRE_RANGE)
                    {
                        float fAmount = 1 - (distanceToUnit / Unit.FIRE_RANGE);
                        if (friendlyTeam.EnemyTeam.Units.Contains(unit))
                        {
                            score += fAmount;
                        }
                        if (friendlyTeam.Units.Contains(unit))
                        {
                            score -= fAmount;
                        }
                    }
                }
                score /= MaxTeamUnitCount;
                influenceMap.Add(node, (float)Math.Round(score, 2));
            }
        }
        private void GenerateFirePowerMap()
        {
            if (!Initialized) return;
            firePowerMap.Clear();
            float score = 0;
            foreach (Battlefield.Node node in Battlefield.Instance.Nodes)
            {
                score = 0;
                foreach (Unit unit in friendlyTeam.EnemyTeam.Units)
                {
                    if (Vector3.Distance(node.WorldPosition, unit.transform.position) <= Unit.FIRE_RANGE)
                    {
                        score += 1;
                    }
                }
                score /= MaxTeamUnitCount;
                firePowerMap.Add(node, (float)Math.Round(score, 2));
            }
        }
        // this can contain multiples of the same tile
        private void CacheCoverNodes()
        {
            if (coverNodes.Count == 0)
            {
                foreach (Battlefield.Node node in Battlefield.Instance.Nodes)
                {
                    if (Battlefield.Instance.HasAnyCoverAt(node) && !coverNodes.Contains(node))
                    {
                        coverNodes.Add(node);
                    }
                }
            }
        }
        private void GenerateCoverMap()
        {
            if (!Initialized) return;
            coverMap.Clear();
            CacheCoverNodes();
            int inRangeCount;
            float score = 0;

            foreach (Battlefield.Node node in coverNodes)
            {
                score = 0;
                inRangeCount = 0;
                List<Vector3> rockPositions = new List<Vector3>();

                foreach (Vector2Int dir in cardinalDirections)
                {
                    if (Battlefield.Instance[node.Position + dir] is Node_Rock rock)
                    {
                        rockPositions.Add(rock.WorldPosition);
                    }
                }


                foreach (Vector3 rockPosition in rockPositions)
                {
                    Vector3 nodeToRock = (rockPosition - node.WorldPosition);

                    foreach (Unit enemyUnit in friendlyTeam.EnemyTeam.Units)
                    {
                        if (Vector3.Distance(node.WorldPosition, enemyUnit.transform.position) > 10)
                        {
                            continue;
                        }
                        Vector3 nodeToEnemy = (enemyUnit.transform.position - node.WorldPosition);
                        float dot = Vector3.Dot(nodeToEnemy.normalized, nodeToRock.normalized);
                        score += ((1 - dot) / 2);
                        inRangeCount++;
                    }
                    if (inRangeCount == 0) { continue; }
                    score /= inRangeCount;
                }
                score /= rockPositions.Count;
                coverMap.Add(new BfNode(node, (float)Math.Round(score, 3)));
            }
        }
        //Works but i don't know whether it provides a good map
        private void GenerateExposedMap()
        {
            float score;
            float neighbourScore;
            HashSet<Battlefield.Node> neighbours;
            foreach (Battlefield.Node node in Battlefield.Instance.Nodes)
            {
                score = 0;
                neighbours = FloodFillInRange(node, exposedRange);
                foreach (Battlefield.Node neighbour in neighbours)
                {
                    neighbourScore = 1 - (Vector3.Distance(node.WorldPosition, neighbour.WorldPosition) / exposedRange);

                    if (Battlefield.Instance.HasAnyCoverAt(node))
                    {
                        score -= neighbourScore;
                    }
                    else
                    {
                        score += neighbourScore;
                    }
                }
                score /= (neighbours.Count - 1);
                exposedMap.Add(node, (float)Math.Round(score, 2));
            }
        }

        HashSet<Battlefield.Node> FloodFillInRange(Battlefield.Node startNode, float range)
        {
            List<Battlefield.Node> openSet = new List<Battlefield.Node>();
            HashSet<Battlefield.Node> closedSet = new HashSet<Battlefield.Node>();

            Battlefield.Node currentNode;
            openSet.Add(startNode);
            while (openSet.Count > 0)
            {
                currentNode = openSet[0];
                openSet.RemoveAt(0);
                closedSet.Add(currentNode);


                foreach (Battlefield.Link link in currentNode.Links)
                {
                    if (!closedSet.Contains(link.Target) && Vector3.Distance(startNode.WorldPosition, link.Target.WorldPosition) <= range)
                    {
                        openSet.Add(link.Target);
                    }
                }
            }
            return closedSet;
        }

        public Battlefield.Node GetBestCoverInRange(Unit unit,float range) 
        {
            ClearNode(unit);
            BfNode returnNode = null;
            float distance;
            foreach (BfNode node in CoverMap) 
            {
                if (node.score == 0 || ActiveNodes.ContainsValue(node.node)) { continue; }
                distance = Vector3.Distance(unit.CurrentNode.WorldPosition,node.node.WorldPosition);
                if(distance > range) { continue; }
                if(returnNode == null){ returnNode = node; continue; }
                if(node.score < returnNode.score) 
                {
                    returnNode = node;
                }
            }

            if (returnNode == null) 
            {
                return null;
            }
            ActiveNodes.Add(unit, returnNode.node);
            return returnNode.node;
        }
        public Battlefield.Node GetLowestScoreInRange(Unit unit,Battlefield.Node target,float range, Dictionary<Battlefield.Node, float> map) 
        {
            ClearNode(unit);
            Battlefield.Node returnNode = null;
            float lowestScore = float.MaxValue;
            float distance;
            foreach(KeyValuePair<Battlefield.Node, float> pair in map) 
            {
                if(pair.Value == 0 || ActiveNodes.ContainsValue(pair.Key)) 
                {
                    continue;
                }
                distance = Vector3.Distance(target.WorldPosition, pair.Key.WorldPosition);
                if(distance > range) { continue; }
                if(returnNode == null) { returnNode = pair.Key; lowestScore = pair.Value; continue; }
                if (pair.Value < lowestScore) 
                {
                    returnNode = pair.Key;
                    lowestScore = pair.Value;
                }
            }

            if (returnNode == null)
            {
                return null;
            }
            ActiveNodes.Add(unit, returnNode);
            return returnNode;
        }
        public void ClearNode(Unit unit) 
        {
            ActiveNodes.Remove(unit);
        }
        public static float GetCompoundedCost(Dictionary<Battlefield.Node, float>[] additionalCosts, Battlefield.Node node, float[] additionalcostMultipliers = null) 
        {
            float additionalCost = 0;
            if (additionalCosts != null)
            {
                for (int i = 0; i < additionalCosts.Length; i++)
                {
                    try 
                    {
                        float multiplier = (additionalcostMultipliers == null || additionalcostMultipliers.Length == 0) ? 1 : (i < additionalcostMultipliers.Length - 1) ? additionalcostMultipliers[i] : 1;
                        additionalCost += additionalCosts[i][node] * multiplier;
                    }
                    catch 
                    {

                    }
                }
            }
            return additionalCost;
        }


        [SerializeField] bool DrawCoverMap = false;
        [SerializeField] bool DrawFirePowerMap = true;
        [SerializeField] bool DrawInfluenceMap = false;
        [SerializeField] bool DrawExposedMap = false;
        private void OnDrawGizmos()
        {
            if (!Initialized) return;
            if (DrawCoverMap) 
            {
                DrawMap(CoverMap);
                return;
            }
            var MapToDraw = (DrawFirePowerMap) ? FirePowerMap : (DrawInfluenceMap) ? InfluenceMap : (DrawExposedMap) ? ExposedMap : null;
            if (MapToDraw != null) 
            {
                DrawMap(MapToDraw);
            }
        }

        private void DrawMap(Dictionary<Battlefield.Node,float> map) 
        {
            foreach (KeyValuePair<Battlefield.Node, float> node in map)
            {
                if (node.Value == 0) continue;
                float score = node.Value;
                Handles.color = (score <= 0.22 && score > 0) ? Color.green : (score < 0.5 && score > 0) ? Color.yellow : (score < 1 && score > 0) ? Color.red : (score < 0 && score > -0.3) ? Color.blue : (score < 0 && score > -0.6) ? Color.cyan : Color.white;
                Handles.DrawSolidDisc(node.Key.WorldPosition, Vector3.up, 0.2f);
                Handles.Label(node.Key.WorldPosition, node.Value.ToString());
            }
        }
        private void DrawMap(HashSet<BfNode> map) 
        {
            foreach (BfNode node in map)
            {
                float score = node.score;
                Handles.color = (score < 0.1 && score > 0) ? Color.green : (score < 0.5 && score > 0) ? Color.yellow : Color.red;
                Handles.DrawSolidDisc(node.node.WorldPosition, Vector3.up, 0.2f);
                Handles.Label(node.node.WorldPosition, node.score.ToString());
            }
        }
    }
}
