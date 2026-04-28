using System.Collections.Generic;

namespace Italbytz.AI.Search.Adversarial;

/// <summary>
/// Represents a turn-based adversarial game for minimax-style search.
/// </summary>
public interface IGame<TState, TAction, TPlayer>
{
    TState InitialState { get; }

    TPlayer Player(TState state);

    IEnumerable<TAction> Actions(TState state);

    TState Result(TState state, TAction action);

    bool Terminal(TState state);

    double Utility(TState state, TPlayer player);
}