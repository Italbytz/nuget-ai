using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Search.Adversarial;

/// <summary>
/// Interface for adversarial search algorithms that choose the next move in a game.
/// </summary>
public interface IAdversarialSearch<TState, TAction>
{
    IMetrics Metrics { get; }

    TAction? MakeDecision(TState state);
}