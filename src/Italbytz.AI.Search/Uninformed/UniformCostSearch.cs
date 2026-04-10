using Italbytz.AI.Search.Framework;
using Italbytz.AI.Search.Framework.QSearch;

namespace Italbytz.AI.Search.Uninformed;

/// <summary>
/// Uniform-cost search that expands the currently cheapest frontier node.
/// </summary>
public class UniformCostSearch<TState, TAction> : QueueBasedSearch<TState, TAction>
    where TState : notnull
{
    public UniformCostSearch() : this(new GraphSearch<TState, TAction>())
    {
    }

    private UniformCostSearch(QueueSearch<TState, TAction> implementation)
        : base(implementation, QueueFactory.CreatePriorityQueue<TState, TAction>(node => node.PathCost))
    {
    }
}
