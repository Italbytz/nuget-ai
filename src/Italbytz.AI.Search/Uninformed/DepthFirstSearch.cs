using Italbytz.AI.Abstractions;
using Italbytz.AI.Problem;

namespace Italbytz.AI.Search.Uninformed;

public class DepthFirstSearch<TState, TAction> : ISearchForActions<TState, TAction>, ISearchForStates<TState, TAction>
    where TState : notnull
{
    private readonly DepthLimitedSearch<TState, TAction> _inner = new(int.MaxValue);

    public IMetrics Metrics => _inner.Metrics;

    public IEnumerable<TAction>? FindActions(IProblem<TState, TAction> problem) => _inner.FindActions(problem);

    public TState? FindState(IProblem<TState, TAction> problem) => _inner.FindState(problem);
}