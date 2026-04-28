using Italbytz.AI.Search.Adversarial;

namespace Italbytz.AI.Demos.Web.Demos;

internal enum TicTacToePlayer
{
    X,
    O
}

internal sealed record TicTacToeMove(int Index)
{
    public int Row => Index / 3;
    public int Column => Index % 3;
}

internal sealed record TicTacToeState(string Board, TicTacToePlayer NextPlayer)
{
    public char GetCell(int index) => Board[index];
}

internal sealed record TicTacToeScenario(
    string Key,
    string Name,
    string Summary,
    TicTacToeState InitialState);

internal sealed record TicTacToeAlgorithmAnalysis(
    string Name,
    TicTacToeMove? BestMove,
    int NodesExpanded,
    double ExpectedUtility,
    string OutcomeLabel);

internal sealed record TicTacToeComparison(
    TicTacToeAlgorithmAnalysis Minimax,
    TicTacToeAlgorithmAnalysis AlphaBeta,
    bool IsTerminal,
    TicTacToePlayer? Winner,
    IReadOnlyList<TicTacToeMove> LegalMoves,
    string PositionSummary,
    IReadOnlyList<TicTacToeBranchInsight> BranchInsights,
    IReadOnlyList<TicTacToeMove> PrincipalVariation,
    int EstimatedSubtreeSavings);

internal sealed record TicTacToeBranchInsight(
    TicTacToeMove Move,
    double Utility,
    int MinimaxNodes,
    int AlphaBetaNodes,
    bool OnPrincipalVariation);

internal static class TicTacToeDemoFactory
{
    private static readonly TicTacToeGame Game = new();

    public static IReadOnlyList<TicTacToeScenario> BuildScenarios()
    {
        return
        [
            new(
                "ForkRace",
                "Fork race",
                "O to move after X has occupied two opposite corners. This is a compact mid-game state where pruning already reduces the search effort.",
                new TicTacToeState("X...O...X", TicTacToePlayer.O)),
            new(
                "ImmediateBlock",
                "Immediate block",
                "O must prevent X from converting the top row immediately. Both algorithms should agree on the defensive move.",
                new TicTacToeState("XX.O..XO.", TicTacToePlayer.O)),
            new(
                "WinningFinish",
                "Winning finish",
                "O to move, but X already threatens two winning lines simultaneously. O can block one at most, so X wins on the very next move regardless. Both algorithms identify the forced win instantly.",
                new TicTacToeState("XO.OX.X..", TicTacToePlayer.O))
        ];
    }

    public static TicTacToeComparison Analyze(TicTacToeState state)
    {
        var minimax = new MinimaxSearch<TicTacToeState, TicTacToeMove, TicTacToePlayer>(Game);
        var alphaBeta = new AlphaBetaSearch<TicTacToeState, TicTacToeMove, TicTacToePlayer>(Game);

        var minimaxMove = minimax.MakeDecision(state);
        var alphaBetaMove = alphaBeta.MakeDecision(state);
        var legalMoves = Game.Actions(state).ToArray();
        var expectedUtility = EvaluateState(state, state.NextPlayer);
        var winner = Game.GetWinner(state);
        var principalVariation = BuildPrincipalVariation(state);
        var branchInsights = BuildBranchInsights(state, principalVariation);
        var estimatedSubtreeSavings = branchInsights.Sum(branch => branch.MinimaxNodes - branch.AlphaBetaNodes);

        return new TicTacToeComparison(
            CreateAnalysis("Minimax", minimaxMove, minimax.Metrics.GetInt(MinimaxSearch<TicTacToeState, TicTacToeMove, TicTacToePlayer>.MetricNodesExpanded), expectedUtility, state.NextPlayer),
            CreateAnalysis("Alpha-beta pruning", alphaBetaMove, alphaBeta.Metrics.GetInt(AlphaBetaSearch<TicTacToeState, TicTacToeMove, TicTacToePlayer>.MetricNodesExpanded), expectedUtility, state.NextPlayer),
            Game.Terminal(state),
            winner,
            legalMoves,
            BuildPositionSummary(state, expectedUtility, winner),
            branchInsights,
            principalVariation,
            estimatedSubtreeSavings);
    }

    public static TicTacToeState ApplyMove(TicTacToeState state, TicTacToeMove move)
    {
        return Game.Result(state, move);
    }

    public static string FormatPlayer(TicTacToePlayer player) => player == TicTacToePlayer.X ? "X" : "O";

    public static string FormatMove(TicTacToeMove? move)
    {
        return move is null ? "none" : $"row {move.Row + 1}, column {move.Column + 1}";
    }

    public static string DescribeCell(char cell)
    {
        return cell switch
        {
            'X' => "X",
            'O' => "O",
            _ => string.Empty
        };
    }

    private static TicTacToeAlgorithmAnalysis CreateAnalysis(string name, TicTacToeMove? move, int nodesExpanded, double expectedUtility, TicTacToePlayer rootPlayer)
    {
        return new TicTacToeAlgorithmAnalysis(name, move, nodesExpanded, expectedUtility, DescribeOutcome(expectedUtility, rootPlayer));
    }

    private static string BuildPositionSummary(TicTacToeState state, double expectedUtility, TicTacToePlayer? winner)
    {
        if (winner is not null)
        {
            return $"Terminal position: {FormatPlayer(winner.Value)} has already won.";
        }

        if (Game.Terminal(state))
        {
            return "Terminal position: optimal play ends in a draw.";
        }

        return DescribeOutcome(expectedUtility, state.NextPlayer);
    }

    private static string DescribeOutcome(double expectedUtility, TicTacToePlayer rootPlayer)
    {
        if (expectedUtility > 0.5)
        {
            return $"{FormatPlayer(rootPlayer)} can force a win.";
        }

        if (expectedUtility < -0.5)
        {
            return $"{FormatPlayer(rootPlayer)} will lose against perfect play.";
        }

        return "Perfect play leads to a draw.";
    }

    private static double EvaluateState(TicTacToeState state, TicTacToePlayer maximizingPlayer)
    {
        if (Game.Terminal(state))
        {
            return Game.Utility(state, maximizingPlayer);
        }

        var nextPlayer = Game.Player(state);
        var values = Game.Actions(state)
            .Select(action => EvaluateState(Game.Result(state, action), maximizingPlayer))
            .ToArray();

        return nextPlayer == maximizingPlayer ? values.Max() : values.Min();
    }

    private static IReadOnlyList<TicTacToeMove> BuildPrincipalVariation(TicTacToeState state)
    {
        if (Game.Terminal(state))
        {
            return [];
        }

        var rootPlayer = state.NextPlayer;
        var variation = new List<TicTacToeMove>();
        var current = state;

        while (!Game.Terminal(current))
        {
            var actions = Game.Actions(current).ToArray();
            if (actions.Length == 0)
            {
                break;
            }

            var isMaxTurn = Game.Player(current) == rootPlayer;
            var scored = actions
                .Select(action => new
                {
                    Action = action,
                    Value = EvaluateState(Game.Result(current, action), rootPlayer)
                })
                .ToArray();

            var bestValue = isMaxTurn ? scored.Max(entry => entry.Value) : scored.Min(entry => entry.Value);
            var bestAction = scored.First(entry => Math.Abs(entry.Value - bestValue) < 0.0001).Action;

            variation.Add(bestAction);
            current = Game.Result(current, bestAction);
        }

        return variation;
    }

    private static IReadOnlyList<TicTacToeBranchInsight> BuildBranchInsights(TicTacToeState state, IReadOnlyList<TicTacToeMove> principalVariation)
    {
        if (Game.Terminal(state))
        {
            return [];
        }

        var rootPlayer = state.NextPlayer;
        var principalFirstMove = principalVariation.FirstOrDefault();
        var actions = Game.Actions(state).ToArray();

        return actions
            .Select(action =>
            {
                var successor = Game.Result(state, action);
                var minimaxNodes = 0;
                var alphaBetaNodes = 0;
                var utility = EvaluateStateWithCount(successor, rootPlayer, ref minimaxNodes);
                _ = EvaluateAlphaBetaWithCount(successor, rootPlayer, double.NegativeInfinity, double.PositiveInfinity, ref alphaBetaNodes);

                return new TicTacToeBranchInsight(
                    action,
                    utility,
                    minimaxNodes,
                    alphaBetaNodes,
                    principalFirstMove is not null && principalFirstMove.Index == action.Index);
            })
            .ToArray();
    }

    private static double EvaluateStateWithCount(TicTacToeState state, TicTacToePlayer maximizingPlayer, ref int expandedNodes)
    {
        if (Game.Terminal(state))
        {
            return Game.Utility(state, maximizingPlayer);
        }

        expandedNodes++;
        var nextPlayer = Game.Player(state);
        var values = new List<double>();
        foreach (var action in Game.Actions(state))
        {
            values.Add(EvaluateStateWithCount(Game.Result(state, action), maximizingPlayer, ref expandedNodes));
        }

        return nextPlayer == maximizingPlayer ? values.Max() : values.Min();
    }

    private static double EvaluateAlphaBetaWithCount(TicTacToeState state, TicTacToePlayer maximizingPlayer, double alpha, double beta, ref int expandedNodes)
    {
        if (Game.Terminal(state))
        {
            return Game.Utility(state, maximizingPlayer);
        }

        expandedNodes++;
        var nextPlayer = Game.Player(state);
        if (nextPlayer == maximizingPlayer)
        {
            var value = double.NegativeInfinity;
            foreach (var action in Game.Actions(state))
            {
                value = Math.Max(value, EvaluateAlphaBetaWithCount(Game.Result(state, action), maximizingPlayer, alpha, beta, ref expandedNodes));
                alpha = Math.Max(alpha, value);
                if (alpha >= beta)
                {
                    break;
                }
            }

            return value;
        }

        var minValue = double.PositiveInfinity;
        foreach (var action in Game.Actions(state))
        {
            minValue = Math.Min(minValue, EvaluateAlphaBetaWithCount(Game.Result(state, action), maximizingPlayer, alpha, beta, ref expandedNodes));
            beta = Math.Min(beta, minValue);
            if (alpha >= beta)
            {
                break;
            }
        }

        return minValue;
    }

    private sealed class TicTacToeGame : IGame<TicTacToeState, TicTacToeMove, TicTacToePlayer>
    {
        private static readonly int[] PreferredActionOrder = [4, 0, 2, 6, 8, 1, 3, 5, 7];
        private static readonly (int A, int B, int C)[] WinningLines =
        [
            (0, 1, 2),
            (3, 4, 5),
            (6, 7, 8),
            (0, 3, 6),
            (1, 4, 7),
            (2, 5, 8),
            (0, 4, 8),
            (2, 4, 6)
        ];

        public TicTacToeState InitialState { get; } = new(".........", TicTacToePlayer.X);

        public TicTacToePlayer Player(TicTacToeState state) => state.NextPlayer;

        public IEnumerable<TicTacToeMove> Actions(TicTacToeState state)
        {
            if (Terminal(state))
            {
                return [];
            }

            return PreferredActionOrder
                .Where(index => state.Board[index] == '.')
                .Select(index => new TicTacToeMove(index));
        }

        public TicTacToeState Result(TicTacToeState state, TicTacToeMove action)
        {
            if (state.Board[action.Index] != '.')
            {
                throw new InvalidOperationException($"Cell {action.Index} is already occupied.");
            }

            var marker = state.NextPlayer == TicTacToePlayer.X ? 'X' : 'O';
            var updatedBoard = state.Board.ToCharArray();
            updatedBoard[action.Index] = marker;

            return new TicTacToeState(new string(updatedBoard), state.NextPlayer == TicTacToePlayer.X ? TicTacToePlayer.O : TicTacToePlayer.X);
        }

        public bool Terminal(TicTacToeState state)
        {
            return GetWinner(state) is not null || state.Board.All(cell => cell != '.');
        }

        public double Utility(TicTacToeState state, TicTacToePlayer player)
        {
            var winner = GetWinner(state);
            if (winner is null)
            {
                return 0;
            }

            return winner == player ? 1 : -1;
        }

        public TicTacToePlayer? GetWinner(TicTacToeState state)
        {
            foreach (var (a, b, c) in WinningLines)
            {
                var first = state.Board[a];
                if (first == '.' || first != state.Board[b] || first != state.Board[c])
                {
                    continue;
                }

                return first == 'X' ? TicTacToePlayer.X : TicTacToePlayer.O;
            }

            return null;
        }
    }
}