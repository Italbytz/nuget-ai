using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Search.Adversarial;

/// <summary>
/// Computes minimax decisions with alpha-beta pruning.
/// </summary>
public class AlphaBetaSearch<TState, TAction, TPlayer> : IAdversarialSearch<TState, TAction>
{
    public const string MetricNodesExpanded = "nodesExpanded";

    private readonly IGame<TState, TAction, TPlayer> _game;

    public AlphaBetaSearch(IGame<TState, TAction, TPlayer> game)
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
            var value = MinValue(_game.Result(state, action), player, double.NegativeInfinity, double.PositiveInfinity);
            if (value <= resultValue)
            {
                continue;
            }

            result = action;
            resultValue = value;
        }

        return result;
    }

    private double MinValue(TState state, TPlayer player, double alpha, double beta)
    {
        Metrics.IncrementInt(MetricNodesExpanded);
        if (_game.Terminal(state))
        {
            return _game.Utility(state, player);
        }

        var value = double.PositiveInfinity;
        foreach (var action in _game.Actions(state))
        {
            value = Math.Min(value, MaxValue(_game.Result(state, action), player, alpha, beta));
            if (value <= alpha)
            {
                return value;
            }

            beta = Math.Min(beta, value);
        }

        return value;
    }

    private double MaxValue(TState state, TPlayer player, double alpha, double beta)
    {
        Metrics.IncrementInt(MetricNodesExpanded);
        if (_game.Terminal(state))
        {
            return _game.Utility(state, player);
        }

        var value = double.NegativeInfinity;
        foreach (var action in _game.Actions(state))
        {
            value = Math.Max(value, MinValue(_game.Result(state, action), player, alpha, beta));
            if (value >= beta)
            {
                return value;
            }

            alpha = Math.Max(alpha, value);
        }

        return value;
    }
}