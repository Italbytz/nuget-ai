using System.Collections.Generic;

namespace Italbytz.AI.Probability;

/// <summary>
/// A Markov Decision Process with states, actions, a stochastic transition model,
/// a reward function, and a discount factor (AIMA3e p. 647).
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TAction">Action type.</typeparam>
public interface IMarkovDecisionProcess<TState, TAction>
{
    IReadOnlyList<TState> States { get; }

    IReadOnlyList<TAction> Actions(TState state);

    /// <summary>P(nextState | state, action).</summary>
    double Transition(TState state, TAction action, TState nextState);

    double Reward(TState state);

    double Discount { get; }
}
