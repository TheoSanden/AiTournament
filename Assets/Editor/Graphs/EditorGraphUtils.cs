using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Graphs
{
    public static class EditorGraphUtils 
    {
        public static void DrawGraph(IGraph graph)
        {
            foreach (INode node in graph.Nodes)
            {
                if (node is IPositionNode source)
                {
                    // draw node position
                    Handles.color = Color.yellow;
                    Handles.CubeHandleCap(0, source.WorldPosition, Quaternion.identity, 0.1f, EventType.Repaint);

                    // draw node links
                    foreach (ILink link in source.Links)
                    {
                        if (link.Target is IPositionNode target)
                        {
                            Handles.color = Color.blue;
                            Handles.DrawLine(source.WorldPosition, target.WorldPosition);
                        }
                    }
                }
            }
        }
    }
}