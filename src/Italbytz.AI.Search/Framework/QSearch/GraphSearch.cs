using System.Collections.Generic;
using Italbytz.AI.Problem;

namespace Italbytz.AI.Search.Framework.QSearch;

/// <summary>
/// Tree search variant that avoids revisiting already explored states.
/// </summary>
public class GraphSearch<TState, TAction> : TreeSearch<TState, TAction>
    where TState : notnull
{
    private readonly HashSet<TState> _explored = [];

    public override INode<TState, TAction>? FindNode(IProblem<TState, TAction> problem, NodePriorityQueue<TState, TAction> frontier)
    {
        _explored.Clear();
        return base.FindNode(problem, frontier);
    }

    protected override void AddToFrontier(NodePriorityQueue<TState, TAction> frontier, INode<TState, TAction> node)
    {
        if (_explored.Contains(node.State))
        {
            return;
        }

        base.AddToFrontier(frontier, node);
    }

    protected override INode<TState, TAction> RemoveFromFrontier(NodePriorityQueue<TState, TAction> frontier)
    {
        var result = frontier.Dequeue();
        _explored.Add(result.State);
        UpdateMetrics(frontier.Count);
        return result;
    }

    protected override bool IsFrontierEmpty(NodePriorityQueue<TState, TAction> frontier)
    {
        while (frontier.Count > 0 && _explored.Contains(frontier.Peek().State))
        {
            frontier.Dequeue();
        }

        UpdateMetrics(frontier.Count);
        return base.IsFrontierEmpty(frontier);
    }
}
