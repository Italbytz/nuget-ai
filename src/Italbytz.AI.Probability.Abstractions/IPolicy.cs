namespace Italbytz.AI.Probability;

/// <summary>
/// A mapping from states to actions — the solution to an MDP (AIMA3e p. 647).
/// </summary>
/// <typeparam name="TState">State type.</typeparam>
/// <typeparam name="TAction">Action type.</typeparam>
public interface IPolicy<TState, TAction>
{
    TAction? Action(TState state);
}
