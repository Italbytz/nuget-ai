using System.Collections.Generic;
using Italbytz.AI.Abstractions;
using Italbytz.AI.Problem;

namespace Italbytz.AI.Search;

/// <summary>
/// Interface for search algorithms that return the actions leading to a goal state.
/// </summary>
public interface ISearchForActions<TState, TAction>
{
    IMetrics Metrics { get; }

    IEnumerable<TAction>? FindActions(IProblem<TState, TAction> problem);
}
