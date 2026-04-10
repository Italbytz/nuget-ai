using System.Collections.Generic;
using Italbytz.AI.Abstractions;
using Italbytz.AI.Problem;
using Italbytz.AI.Search.Framework.QSearch;

namespace Italbytz.AI.Search.Framework;

/// <summary>
/// Base class for queue-based search algorithms.
/// </summary>
public abstract class QueueBasedSearch<TState, TAction> : ISearchForActions<TState, TAction>, ISearchForStates<TState, TAction>
{
    private readonly NodePriorityQueue<TState, TAction> _frontier;
    private readonly QueueSearch<TState, TAction> _implementation;

    protected QueueBasedSearch(QueueSearch<TState, TAction> implementation, NodePriorityQueue<TState, TAction> frontier)
    {
        _implementation = implementation;
        _frontier = frontier;
    }

    public IMetrics Metrics => _implementation.Metrics;

    public IEnumerable<TAction>? FindActions(IProblem<TState, TAction> problem)
    {
        _implementation.NodeFactory.UseParentLinks = true;
        _frontier.Clear();
        var node = _implementation.FindNode(problem, _frontier);
        return SearchUtils.ToActions(node);
    }

    public TState? FindState(IProblem<TState, TAction> problem)
    {
        _implementation.NodeFactory.UseParentLinks = false;
        _frontier.Clear();
        var node = _implementation.FindNode(problem, _frontier);
        return node is null ? default : node.State;
    }
}
