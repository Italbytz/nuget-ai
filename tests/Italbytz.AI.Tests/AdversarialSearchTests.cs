using Italbytz.AI.Search.Adversarial;

namespace Italbytz.AI.Tests;

[TestClass]
public class AdversarialSearchTests
{
    [TestMethod]
    public void MinimaxSearchChoosesLegacyBestMoveOnTwoPlyGame()
    {
        var game = new TwoPlyGame();
        var search = new MinimaxSearch<TwoPlyState, string, string>(game);

        var decision = search.MakeDecision(game.InitialState);

        Assert.AreEqual("B", decision);
        Assert.AreEqual(12, search.Metrics.GetInt(MinimaxSearch<TwoPlyState, string, string>.MetricNodesExpanded));
    }

    [TestMethod]
    public void AlphaBetaSearchChoosesLegacyBestMoveOnTwoPlyGame()
    {
        var game = new TwoPlyGame();
        var search = new AlphaBetaSearch<TwoPlyState, string, string>(game);
        var baseline = new MinimaxSearch<TwoPlyState, string, string>(game);

        var decision = search.MakeDecision(game.InitialState);
        baseline.MakeDecision(game.InitialState);

        Assert.AreEqual("B", decision);
        Assert.IsLessThanOrEqualTo(
            search.Metrics.GetInt(AlphaBetaSearch<TwoPlyState, string, string>.MetricNodesExpanded),
            baseline.Metrics.GetInt(MinimaxSearch<TwoPlyState, string, string>.MetricNodesExpanded));
    }

    private sealed record TwoPlyState(string Label);

    private sealed class TwoPlyGame : IGame<TwoPlyState, string, string>
    {
        private static readonly IReadOnlyDictionary<string, string[]> ActionsByState = new Dictionary<string, string[]>
        {
            ["A"] = ["B", "C", "D"],
            ["B"] = ["E", "F", "G"],
            ["C"] = ["H", "I", "J"],
            ["D"] = ["K", "L", "M"],
            ["E"] = [],
            ["F"] = [],
            ["G"] = [],
            ["H"] = [],
            ["I"] = [],
            ["J"] = [],
            ["K"] = [],
            ["L"] = [],
            ["M"] = []
        };

        private static readonly IReadOnlyDictionary<string, double> Utilities = new Dictionary<string, double>
        {
            ["E"] = 3,
            ["F"] = 12,
            ["G"] = 8,
            ["H"] = 2,
            ["I"] = 4,
            ["J"] = 6,
            ["K"] = 14,
            ["L"] = 5,
            ["M"] = 2
        };

        public TwoPlyState InitialState { get; } = new("A");

        public string Player(TwoPlyState state)
        {
            return state.Label is "A" ? "MAX" : Utilities.ContainsKey(state.Label) ? "TERMINAL" : "MIN";
        }

        public IEnumerable<string> Actions(TwoPlyState state)
        {
            return ActionsByState[state.Label];
        }

        public TwoPlyState Result(TwoPlyState state, string action)
        {
            return new TwoPlyState(action);
        }

        public bool Terminal(TwoPlyState state)
        {
            return Utilities.ContainsKey(state.Label);
        }

        public double Utility(TwoPlyState state, string player)
        {
            return Utilities[state.Label];
        }
    }
}