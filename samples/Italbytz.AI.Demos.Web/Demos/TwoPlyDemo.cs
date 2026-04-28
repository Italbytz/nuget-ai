using Italbytz.AI.Search.Adversarial;

namespace Italbytz.AI.Demos.Web.Demos;

internal enum TwoPlyPlayer
{
    Max,
    Min
}

internal sealed record TwoPlyMove(string Label, string ToStateId);

internal sealed record TwoPlyState(string Id, int Depth);

internal sealed record TwoPlyBranchSummary(string ActionLabel, double MinimaxValue, bool Recommended);

internal sealed record TwoPlyAlgorithmAnalysis(
    string Name,
    TwoPlyMove? BestMove,
    int NodesExpanded,
    double RootValue);

internal sealed record TwoPlyComparison(
    TwoPlyScenario Scenario,
    TwoPlyAlgorithmAnalysis Minimax,
    TwoPlyAlgorithmAnalysis AlphaBeta,
    IReadOnlyList<TwoPlyBranchSummary> Branches,
    string Explanation,
    IReadOnlyList<TwoPlyLeafInsight> LeafInsights,
    IReadOnlyList<string> PrincipalVariation,
    int AlphaBetaPrunedLeafCount);

internal sealed record TwoPlyLeafInsight(
    string RootAction,
    string Leaf,
    double Utility,
    bool EvaluatedByAlphaBeta);

internal sealed record TwoPlyScenario(
    string Key,
    string Name,
    string Summary,
    IReadOnlyList<string> RootOrder,
    IReadOnlyDictionary<string, IReadOnlyList<string>> ChildOrder);

internal static class TwoPlyDemoFactory
{
    public static IReadOnlyList<TwoPlyScenario> BuildScenarios()
    {
        return
        [
            new TwoPlyScenario(
                "NaturalOrder",
                "Natural order",
                "Standard left-to-right ordering. Alpha-beta still prunes, but only after evaluating several MIN children.",
                ["B", "C", "D"],
                new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
                {
                    ["B"] = ["B1", "B2", "B3"],
                    ["C"] = ["C1", "C2", "C3"],
                    ["D"] = ["D1", "D2", "D3"]
                }),
            new TwoPlyScenario(
                "PruningFriendly",
                "Pruning-friendly order",
                "The MIN children are ordered so that low utilities are discovered early, which allows alpha-beta to cut off more branches.",
                ["B", "D", "C"],
                new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)
                {
                    ["B"] = ["B1", "B2", "B3"],
                    ["C"] = ["C1", "C2", "C3"],
                    ["D"] = ["D3", "D2", "D1"]
                })
        ];
    }

    public static TwoPlyComparison Analyze(TwoPlyScenario scenario)
    {
        var game = new TwoPlyGame(scenario);
        var root = game.InitialState;

        var minimax = new MinimaxSearch<TwoPlyState, TwoPlyMove, TwoPlyPlayer>(game);
        var alphaBeta = new AlphaBetaSearch<TwoPlyState, TwoPlyMove, TwoPlyPlayer>(game);

        var minimaxMove = minimax.MakeDecision(root);
        var alphaBetaMove = alphaBeta.MakeDecision(root);
        var rootPlayer = game.Player(root);
        var rootValue = EvaluateValue(game, root, rootPlayer);

        var branches = game.Actions(root)
            .Select(action =>
            {
                var childValue = EvaluateValue(game, game.Result(root, action), rootPlayer);
                return new TwoPlyBranchSummary(action.Label, childValue, string.Equals(action.Label, minimaxMove?.Label, StringComparison.Ordinal));
            })
            .ToArray();

        var explanation = minimaxMove?.Label == alphaBetaMove?.Label
            ? "Both algorithms select the same optimal root action; alpha-beta only changes how many nodes must be expanded."
            : "The selected moves differ, which indicates either equivalent utilities with tie-breaking or a modeling mismatch.";

        var leafInsights = BuildLeafInsights(scenario);
        var prunedLeafCount = leafInsights.Count(leaf => !leaf.EvaluatedByAlphaBeta);
        var principalVariation = BuildPrincipalVariation(leafInsights, minimaxMove?.Label);

        return new TwoPlyComparison(
            scenario,
            new TwoPlyAlgorithmAnalysis(
                "Minimax",
                minimaxMove,
                minimax.Metrics.GetInt(MinimaxSearch<TwoPlyState, TwoPlyMove, TwoPlyPlayer>.MetricNodesExpanded),
                rootValue),
            new TwoPlyAlgorithmAnalysis(
                "Alpha-beta pruning",
                alphaBetaMove,
                alphaBeta.Metrics.GetInt(AlphaBetaSearch<TwoPlyState, TwoPlyMove, TwoPlyPlayer>.MetricNodesExpanded),
                rootValue),
            branches,
            explanation,
            leafInsights,
            principalVariation,
            prunedLeafCount);
    }

    private static IReadOnlyList<TwoPlyLeafInsight> BuildLeafInsights(TwoPlyScenario scenario)
    {
        var alpha = double.NegativeInfinity;
        var insights = new List<TwoPlyLeafInsight>();

        foreach (var root in scenario.RootOrder)
        {
            var leaves = scenario.ChildOrder[root];
            var branchValue = double.PositiveInfinity;
            var branchCutoff = false;

            foreach (var leaf in leaves)
            {
                var utility = TwoPlyGame.GetUtilityValue(leaf);
                var evaluated = !branchCutoff;
                if (evaluated)
                {
                    branchValue = Math.Min(branchValue, utility);
                    if (branchValue <= alpha)
                    {
                        branchCutoff = true;
                    }
                }

                insights.Add(new TwoPlyLeafInsight(
                    root,
                    leaf,
                    utility,
                    evaluated));
            }

            alpha = Math.Max(alpha, branchValue);
        }

        return insights;
    }

    private static IReadOnlyList<string> BuildPrincipalVariation(IReadOnlyList<TwoPlyLeafInsight> leafInsights, string? bestRootAction)
    {
        if (bestRootAction is null)
        {
            return [];
        }

        var bestLeaf = leafInsights
            .Where(leaf => string.Equals(leaf.RootAction, bestRootAction, StringComparison.Ordinal))
            .OrderBy(leaf => leaf.Utility)
            .ThenBy(leaf => leaf.Leaf, StringComparer.Ordinal)
            .FirstOrDefault();

        return bestLeaf is null
            ? ["A", bestRootAction]
            : ["A", bestRootAction, bestLeaf.Leaf];
    }

    private static double EvaluateValue(TwoPlyGame game, TwoPlyState state, TwoPlyPlayer rootPlayer)
    {
        if (game.Terminal(state))
        {
            return game.Utility(state, rootPlayer);
        }

        var values = game.Actions(state)
            .Select(action => EvaluateValue(game, game.Result(state, action), rootPlayer))
            .ToArray();

        return game.Player(state) == rootPlayer ? values.Max() : values.Min();
    }

    private sealed class TwoPlyGame : IGame<TwoPlyState, TwoPlyMove, TwoPlyPlayer>
    {
        private static readonly IReadOnlyDictionary<string, double> Utilities = new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["B1"] = 3,
            ["B2"] = 12,
            ["B3"] = 8,
            ["C1"] = 2,
            ["C2"] = 4,
            ["C3"] = 6,
            ["D1"] = 14,
            ["D2"] = 5,
            ["D3"] = 2
        };

        private readonly TwoPlyScenario _scenario;

        public TwoPlyGame(TwoPlyScenario scenario)
        {
            _scenario = scenario;
        }

        public static double GetUtilityValue(string stateId)
        {
            return Utilities.TryGetValue(stateId, out var value) ? value : 0;
        }

        public TwoPlyState InitialState { get; } = new("A", 0);

        public TwoPlyPlayer Player(TwoPlyState state)
        {
            return state.Depth % 2 == 0 ? TwoPlyPlayer.Max : TwoPlyPlayer.Min;
        }

        public IEnumerable<TwoPlyMove> Actions(TwoPlyState state)
        {
            if (Terminal(state))
            {
                return [];
            }

            if (state.Id == "A")
            {
                return _scenario.RootOrder.Select(child => new TwoPlyMove(child, child));
            }

            if (_scenario.ChildOrder.TryGetValue(state.Id, out var children))
            {
                return children.Select(child => new TwoPlyMove(child, child));
            }

            return [];
        }

        public TwoPlyState Result(TwoPlyState state, TwoPlyMove action)
        {
            return new TwoPlyState(action.ToStateId, state.Depth + 1);
        }

        public bool Terminal(TwoPlyState state)
        {
            return Utilities.ContainsKey(state.Id);
        }

        public double Utility(TwoPlyState state, TwoPlyPlayer player)
        {
            if (!Utilities.TryGetValue(state.Id, out var value))
            {
                return 0;
            }

            return player == TwoPlyPlayer.Max ? value : -value;
        }
    }
}