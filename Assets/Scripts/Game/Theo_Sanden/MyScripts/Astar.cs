using System.Collections;
using System.Collections.Generic;
using Game;
using Graphs;
using UnityEngine;

namespace Theo_Sanden
{
    public class Astar : MonoBehaviour
    {
        public static GraphUtils.Path GetClosestPath(Battlefield.Node start, Battlefield.Node goal, Dictionary<Battlefield.Node,float>[] additionalCosts = null, float[] additionalCostMultiplier = null)
        {
            if (start == null ||
               goal == null ||
               start == goal ||
               Battlefield.Instance == null)
            {
                return null;
            }

            // initialize pathfinding
            foreach (Battlefield.Node node in Battlefield.Instance.Nodes)
            {
                node?.ResetPathfinding();
            }

            // add start node
            start.m_fDistance = 0.0f;
            start.m_fRemainingDistance = Battlefield.Instance.Heuristic(goal, start);
            List<Battlefield.Node> open = new List<Battlefield.Node>();
            HashSet<Battlefield.Node> closed = new HashSet<Battlefield.Node>();
            open.Add(start);
            // search
            while (open.Count > 0)
            {
                // get next node (the one with the least remaining distance)
                Battlefield.Node current = open[0];
                float currentAdditionalCost = BattleFieldAnalysis.GetCompoundedCost(additionalCosts, current, additionalCostMultiplier); 
                for (int i = 1; i < open.Count; ++i)
                {
                    float iAdditionalCost = BattleFieldAnalysis.GetCompoundedCost(additionalCosts, open[i], additionalCostMultiplier); 
                    if (open[i].m_fRemainingDistance + iAdditionalCost < current.m_fRemainingDistance + currentAdditionalCost)
                    {
                        current = open[i];
                        currentAdditionalCost = iAdditionalCost;
                    }
                }
                open.Remove(current);
                closed.Add(current);

                // found goal?
                if (current == goal)
                {
                    // construct path
                    GraphUtils.Path path = new GraphUtils.Path();
                    while (current != null)
                    {
                        path.Add(current.m_parentLink);
                       // if(current.m_parentLink != null) Debug.DrawLine(current.WorldPosition,current.m_parentLink.Source.WorldPosition,Color.white,2.0f);
                        current = current != null && current.m_parentLink != null ? current.m_parentLink.Source : null;
                    }

                    path.RemoveAll(l => l == null);     // HACK: check if path contains null links
                    path.Reverse();


                    return path;
                }
                else
                {
                    foreach (Battlefield.Link link in current.Links)
                    {
                        if (link.Target is Battlefield.Node target)
                        {
                            if (!closed.Contains(target) &&
                                target.Unit == null)
                            {
                                float newDistance = current.m_fDistance + Vector3.Distance(current.WorldPosition, target.WorldPosition);
                                float newRemainingDistance = newDistance + Battlefield.Instance.Heuristic(target, start);

                                if (open.Contains(target))
                                {
                                    if (newRemainingDistance < target.m_fRemainingDistance)
                                    {
                                        // re-parent neighbor node
                                        target.m_fRemainingDistance = newRemainingDistance;
                                        target.m_fDistance = newDistance;
                                        target.m_parentLink = link;
                                    }
                                }
                                else
                                {
                                    // add target to openlist
                                    target.m_fRemainingDistance = newRemainingDistance;
                                    target.m_fDistance = newDistance;
                                    target.m_parentLink = link;
                                    open.Add(target);
                                }
                            }
                        }
                    }
                }
            }
            // no path found :(
            return null;
        }
    }
}

