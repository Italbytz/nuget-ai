using Italbytz.AI.Problem;

namespace Italbytz.AI.Search;

/// <summary>
/// Interface for search algorithms that return only a resulting state.
/// </summary>
public interface ISearchForStates<TState, TAction>
{
    TState? FindState(IProblem<TState, TAction> problem);
}
