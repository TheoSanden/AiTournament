using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Graphs
{
    public interface INode
    {
        IEnumerable<ILink> Links { get; }
    }

    public interface IPositionNode : INode
    {
        Vector3 WorldPosition { get; }
    }

    public interface ILink
    {
        INode Source { get; }

        INode Target { get; }
    }

    public interface IGraph
    {
        IEnumerable<INode> Nodes { get; }
    }

    public interface ISearchableGraph : IGraph
    {
        float Heuristic(INode start, INode goal);
    }
}