using System.Collections.Generic;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Probability;

/// <summary>
/// Solver for Markov Decision Processes; returns a policy and utility estimates.
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TAction">Action type.</typeparam>
public interface IMdpSolver<TState, TAction>
{
    IMetrics Metrics { get; }

    (IPolicy<TState, TAction> Policy, IReadOnlyDictionary<TState, double> Utilities) Solve(
        IMarkovDecisionProcess<TState, TAction> mdp);
}
