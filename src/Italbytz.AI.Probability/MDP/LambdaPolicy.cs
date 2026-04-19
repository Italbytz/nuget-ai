using System;

namespace Italbytz.AI.Probability.MDP;

/// <summary>Functional wrapper implementing <see cref="IPolicy{TState,TAction}"/>.</summary>
internal sealed class LambdaPolicy<TState, TAction> : IPolicy<TState, TAction>
{
    private readonly Func<TState, TAction?> _fn;
    public LambdaPolicy(Func<TState, TAction?> fn) => _fn = fn;
    public TAction? Action(TState state) => _fn(state);
}
