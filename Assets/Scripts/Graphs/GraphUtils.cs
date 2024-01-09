using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphs
{
    public static class GraphUtils 
    {
        private class ANode<T> where T : class, INode
        {
            public T        m_node;
            public ILink    m_link;
            public float    m_fDistance;
            public float    m_fRemainingDistance;
            public ANode<T> m_parent;

            public ANode(T node)
            {
                m_node = node;
                m_fDistance = float.MaxValue;
                m_fRemainingDistance = float.MaxValue;
            }
        }

        public class Path : List<ILink>
        {            
        }

        public static T GetClosestNode<T>(IGraph graph, Vector3 vWorldPos, float fMaxDistance = float.MaxValue) where T : class, IPositionNode 
        {
            T bestNode = null;
            if (graph != null)
            {
                foreach (INode node in graph.Nodes)
                {
                    if (node is T pn)
                    {
                        float fDistance = Vector3.Distance(vWorldPos, pn.WorldPosition);
                        if (fDistance < fMaxDistance)
                        {
                            fMaxDistance = fDistance;
                            bestNode = pn;
                        }
                    }
                }
            }

            return bestNode;
        }

        public static HashSet<T> FloodFill<T>(IGraph graph, T node) where T : class, INode
        {
            HashSet<T> closed = new HashSet<T>();
            if (graph != null &&
                node != null)
            {
                // add starting node
                Queue<T> open = new Queue<T>();
                open.Enqueue(node);

                while (open.Count > 0)
                {
                    // get next node
                    node = open.Dequeue();
                    closed.Add(node);

                    // search links
                    foreach (Link link in node.Links)
                    {
                        if (link.Target is T target &&
                            !closed.Contains(target) &&
                            !open.Contains(target))
                        {
                            open.Enqueue(target);
                        }
                    }
                }
            }

            return closed;
        }

        public static HashSet<T> GetNodesWithinDistance<T>(IGraph graph, T node, int iDistance) where T : class, INode
        {
            HashSet<T> closed = new HashSet<T>();
            if (graph != null &&
                node != null)
            {
                // add starting node
                Queue<T> open = new Queue<T>();
                open.Enqueue(node);

                for (int i = 0; i <= iDistance; ++i)
                {
                    Queue<T> nextOpen = new Queue<T>();

                    while (open.Count > 0)
                    {
                        // get next node
                        node = open.Dequeue();
                        closed.Add(node);

                        // search links
                        foreach (Link link in node.Links)
                        {
                            if (link.Target is T target &&
                                !closed.Contains(target) &&
                                !open.Contains(target) &&
                                !nextOpen.Contains(target))
                            {
                                nextOpen.Enqueue(target);
                            }
                        }
                    }

                    open = nextOpen;
                }
            }

            return closed;
        }

        public static Path GetShortestPath_Dijkstra<T>(IGraph graph, T start, T goal) where T : class, INode
        {
            if (graph == null ||
                start == null ||
                goal == null)
            {
                return null;
            }

            // create ANode lookup
            Dictionary<T, ANode<T>> nodes = new Dictionary<T, ANode<T>>();
            foreach (INode n in graph.Nodes)
            {
                if (n is T node)
                {
                    nodes[node] = new ANode<T>(node);
                }
            }

            // get start & goal
            ANode<T> startNode, goalNode;
            if (!nodes.TryGetValue(start, out startNode) ||
                !nodes.TryGetValue(goal, out goalNode))
            {
                return null;
            }

            // add goal node
            HashSet<ANode<T>> closed = new HashSet<ANode<T>>();
            List<ANode<T>> open = new List<ANode<T>>();
            startNode.m_fDistance = 0.0f;
            open.Add(startNode);
            bool bIsPositionNodes = typeof(IPositionNode).IsAssignableFrom(typeof(T));

            // search
            while (open.Count > 0)
            {
                // get next node (the one with the least distance)
                ANode<T> current = open[0];
                for (int i = 1; i < open.Count; ++i)
                {
                    if (open[i].m_fDistance < current.m_fDistance)
                    {
                        current = open[i];
                    }
                }
                open.Remove(current);
                closed.Add(current);

                // search links
                foreach (Link link in current.m_node.Links)
                {
                    if (link.Target is T target)
                    {
                        ANode<T> targetNode = nodes[target];
                        if (!closed.Contains(targetNode) &&
                            !open.Contains(targetNode))
                        {
                            // is new path shorter?
                            float fNewDistance = current.m_fDistance + (bIsPositionNodes ? Vector3.Distance((current.m_node as IPositionNode).WorldPosition, (target as IPositionNode).WorldPosition) : 1.0f);
                            if (fNewDistance < targetNode.m_fDistance)
                            {
                                targetNode.m_fDistance = fNewDistance;
                                targetNode.m_parent = current;
                                targetNode.m_link = link;
                                open.Add(targetNode);
                            }
                        }
                    }
                }

                // reached start?
                if (current == goalNode)
                {
                    Path path = new Path();
                    while (current != null)
                    {
                        path.Add(current.m_link);
                        current = current.m_parent;
                    }

                    // got a path!
                    path.RemoveAll(l => l == null);     // HACK: check if path contains null links
                    path.Reverse();
                    return path;
                }
            }

            // no path found :(
            return null;
        }

        public static Path GetShortestPath_AStar<T>(IGraph graph, T start, T goal) where T : class, INode
        {
            if (graph == null ||
                start == null ||
                goal == null)
            {
                return null;
            }

            ISearchableGraph searchableGraph = graph as ISearchableGraph;

            // create A* Node lookup
            Dictionary<T, ANode<T>> nodes = new Dictionary<T, ANode<T>>();
            foreach (INode n in graph.Nodes)
            {
                if (n is T node)
                {
                    nodes[node] = new ANode<T>(node);
                }
            }

            // get start & goal
            ANode<T> startNode, goalNode;
            if (!nodes.TryGetValue(start, out startNode) ||
                !nodes.TryGetValue(goal, out goalNode))
            {
                return null;
            }

            // add start node
            HashSet<ANode<T>> closed = new HashSet<ANode<T>>();
            List<ANode<T>> open = new List<ANode<T>>();
            startNode.m_fDistance = 0.0f;
            startNode.m_fRemainingDistance = searchableGraph != null ? searchableGraph.Heuristic(goal, start) : float.MaxValue;
            open.Add(startNode);
            bool bIsPositionNodes = typeof(IPositionNode).IsAssignableFrom(typeof(T));

            // search
            while (open.Count > 0)
            {
                // get next node (the one with the least remaining distance)
                ANode<T> current = open[0];
                for (int i = 1; i < open.Count; ++i)
                {
                    if (open[i].m_fRemainingDistance < current.m_fRemainingDistance)
                    {
                        current = open[i];
                    }
                }
                open.Remove(current);
                closed.Add(current);

                // found goal?
                if (current == goalNode)
                {
                    // construct path
                    Path path = new Path();
                    while (current != null)
                    {
                        path.Add(current.m_link);
                        current = current.m_parent;
                    }

                    path.RemoveAll(l => l == null);     // HACK: check if path contains null links
                    path.Reverse();
                    return path;
                }
                else
                {
                    foreach (Link link in current.m_node.Links)
                    {
                        if (link.Target is T targetNode)
                        {
                            ANode<T> target = nodes[targetNode];
                            if (!closed.Contains(target))
                            {
                                float newDistance = current.m_fDistance + (searchableGraph != null ? searchableGraph.Heuristic(current.m_node, targetNode) : 1.0f);
                                float newRemainingDistance = newDistance + (searchableGraph != null ? searchableGraph.Heuristic(targetNode, start) : 1.0f);

                                if (closed.Contains(target) ||
                                    open.Contains(target))
                                {
                                    if (newRemainingDistance < target.m_fRemainingDistance)
                                    {
                                        // re-parent neighbor node
                                        target.m_fRemainingDistance = newRemainingDistance;
                                        target.m_fDistance = newDistance;
                                        target.m_parent = current;
                                        target.m_link = link;
                                    }
                                }
                                else
                                {
                                    // add target to openlist
                                    target.m_fRemainingDistance = newRemainingDistance;
                                    target.m_fDistance = newDistance;
                                    target.m_parent = current;
                                    target.m_link = link;
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