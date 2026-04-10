using System;
using Italbytz.AI.Search;

namespace Italbytz.AI.Problem;

/// <summary>
/// Formal problem definition used by search algorithms.
/// </summary>
public interface IProblem<TState, TAction> : IOnlineSearchProblem<TState, TAction>
{
    Func<TState, TAction, TState> Result { get; }

    bool TestSolution(INode<TState, TAction> node);
}
