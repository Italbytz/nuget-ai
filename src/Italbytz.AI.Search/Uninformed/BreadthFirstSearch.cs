using Italbytz.AI.Search.Framework;
using Italbytz.AI.Search.Framework.QSearch;

namespace Italbytz.AI.Search.Uninformed;

/// <summary>
/// Breadth-first search implemented as queue-based tree search ordered by node depth.
/// </summary>
public class BreadthFirstSearch<TState, TAction> : QueueBasedSearch<TState, TAction>
{
    public BreadthFirstSearch() : this(new TreeSearch<TState, TAction>())
    {
    }

    private BreadthFirstSearch(QueueSearch<TState, TAction> implementation)
        : base(implementation, QueueFactory.CreatePriorityQueue<TState, TAction>(node => node.Depth))
    {
    }
}
