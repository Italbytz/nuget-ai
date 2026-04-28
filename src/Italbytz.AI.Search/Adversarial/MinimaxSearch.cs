using System.Linq;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Search.Adversarial;

/// <summary>
/// Computes minimax decisions for finite deterministic turn-based games.
/// </summary>
public class MinimaxSearch<TState, TAction, TPlayer> : IAdversarialSearch<TState, TAction>
{
    public const string MetricNodesExpanded = "nodesExpanded";

    private readonly IGame<TState, TAction, TPlayer> _game;

    public MinimaxSearch(IGame<TState, TAction, TPlayer> game)
    {
        _game = game;
    }

    public IMetrics Metrics { get; private set; } = new Metrics();

    public TAction? MakeDecision(TState state)
    {
        Metrics = new Metrics();
        Metrics.Set(MetricNodesExpanded, 0);

        TAction? result = default;
        var resultValue = double.NegativeInfinity;
        var player = _game.Player(state);

        foreach (var action in _game.Actions(state))
        {
            var value = MinValue(_game.Result(state, action), player);
            if (value <= resultValue)
            {
                continue;
            }

            result = action;
            resultValue = value;
        }

        return result;
    }

    private double MinValue(TState state, TPlayer player)
    {
        Metrics.IncrementInt(MetricNodesExpanded);
        if (_game.Terminal(state))
        {
            return _game.Utility(state, player);
        }

        var actionValues = _game.Actions(state)
            .Select(action => MaxValue(_game.Result(state, action), player))
            .ToList();

        return actionValues.Count > 0 ? actionValues.Min() : double.PositiveInfinity;
    }

    private double MaxValue(TState state, TPlayer player)
    {
        Metrics.IncrementInt(MetricNodesExpanded);
        if (_game.Terminal(state))
        {
            return _game.Utility(state, player);
        }

        var actionValues = _game.Actions(state)
            .Select(action => MinValue(_game.Result(state, action), player))
            .ToList();

        return actionValues.Count > 0 ? actionValues.Max() : double.NegativeInfinity;
    }
}