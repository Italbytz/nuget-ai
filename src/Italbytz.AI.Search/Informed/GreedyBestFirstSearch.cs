using Italbytz.AI.Search.Framework;
using Italbytz.AI.Search.Framework.QSearch;

namespace Italbytz.AI.Search.Informed;

/// <summary>
/// Best-first search that orders the frontier purely by heuristic estimate.
/// </summary>
public class GreedyBestFirstSearch<TState, TAction> : QueueBasedSearch<TState, TAction>
    where TState : notnull
{
    public GreedyBestFirstSearch(Func<INode<TState, TAction>, double> heuristic)
        : this(new GraphSearch<TState, TAction>(), heuristic)
    {
    }

    private GreedyBestFirstSearch(QueueSearch<TState, TAction> implementation, Func<INode<TState, TAction>, double> heuristic)
        : base(implementation, QueueFactory.CreatePriorityQueue(heuristic))
    {
    }
}