using Italbytz.AI.CSP;
using Italbytz.AI.CSP.Solver;

namespace Italbytz.AI.Demos.Web.Demos;

internal enum NQueensAlgorithmKind
{
    HillClimbing,
    SimulatedAnnealing,
    GeneticAlgorithm
}

internal enum NQueensStepKind
{
    Initial,
    Move,
    RejectedMove,
    Generation,
    Solved,
    Stopped
}

internal sealed record NQueensStep(
    int Number,
    string Summary,
    string Metric,
    string Description,
    int[] QueenRows,
    int? CurrentColumn,
    int? CurrentRow,
    IReadOnlyList<int> ConflictedColumns,
    int ConflictCount,
    bool Solved,
    bool Stopped);

internal sealed record NQueensAlgorithmDemo(
    string Key,
    string Name,
    string Summary,
    int BoardSize,
    int[] InitialQueenRows,
    IReadOnlyList<NQueensStep> Steps);

internal sealed record NQueensDemoConfiguration(
    int BoardSize,
    int[] HillClimbingInitialBoard,
    int[] SimulatedAnnealingInitialBoard,
    int GeneticSeed);

internal static class NQueensDemoFactory
{
    private static readonly int[] HillClimbingInitialBoard = [5, 7, 0, 1, 1, 7, 7, 2];
    private static readonly int[] AnnealingInitialBoard = [0, 4, 7, 5, 2, 6, 1, 3];

    public static string PackageReferenceSummary { get; } = BuildPackageReferenceSummary(8);

    public static int CountConflictsForBoard(IReadOnlyList<int> board) => CountConflicts(board);

    public static string FormatFitnessForBoard(IReadOnlyList<int> board) => FormatDouble(Fitness(board));

    public static IReadOnlyList<NQueensAlgorithmDemo> BuildAll(int boardSize = 8)
    {
        return BuildAll(CreateDefaultConfiguration(boardSize));
    }

    public static IReadOnlyList<NQueensAlgorithmDemo> BuildAll(NQueensDemoConfiguration configuration)
    {
        return
        [
            SimulateHillClimbing(configuration),
            SimulateSimulatedAnnealing(configuration),
            SimulateGeneticAlgorithm(configuration)
        ];
    }

    public static NQueensDemoConfiguration CreateRandomConfiguration(int boardSize, Random random)
    {
        return new NQueensDemoConfiguration(
            boardSize,
            CreateInterestingRandomBoard(boardSize, random),
            CreateInterestingRandomBoard(boardSize, random),
            random.Next());
    }

    private static NQueensAlgorithmDemo SimulateHillClimbing(NQueensDemoConfiguration configuration)
    {
        var current = CreateBoard(configuration.BoardSize, configuration.HillClimbingInitialBoard);
        var steps = new List<NQueensStep>
        {
            CreateStep(1, NQueensAlgorithmKind.HillClimbing, NQueensStepKind.Initial, current, null, null, null, null, true, false)
        };

        var stepNumber = 2;
        while (true)
        {
            var currentConflicts = CountConflicts(current);
            if (currentConflicts == 0)
            {
                steps.Add(CreateStep(stepNumber, NQueensAlgorithmKind.HillClimbing, NQueensStepKind.Solved, current, null, null, null, Fitness(current), true, true));
                return new NQueensAlgorithmDemo("HillClimbing", "Hill climbing", "Greedy local search that always applies the best improving queen move.", configuration.BoardSize, CreateBoard(configuration.BoardSize, configuration.HillClimbingInitialBoard), steps);
            }

            var bestMove = FindBestHillClimbingMove(current);
            if (bestMove is null || bestMove.ConflictCount >= currentConflicts)
            {
                steps.Add(CreateStep(stepNumber, NQueensAlgorithmKind.HillClimbing, NQueensStepKind.Stopped, current, null, null, null, Fitness(current), true, false));
                return new NQueensAlgorithmDemo("HillClimbing", "Hill climbing", "Greedy local search that always applies the best improving queen move.", configuration.BoardSize, CreateBoard(configuration.BoardSize, configuration.HillClimbingInitialBoard), steps);
            }

            current[bestMove.Column] = bestMove.Row;
            steps.Add(CreateStep(stepNumber, NQueensAlgorithmKind.HillClimbing, NQueensStepKind.Move, current, bestMove.Column, bestMove.Row, null, Fitness(current), true, CountConflicts(current) == 0));
            stepNumber++;
        }
    }

    private static NQueensAlgorithmDemo SimulateSimulatedAnnealing(NQueensDemoConfiguration configuration)
    {
        var current = CreateBoard(configuration.BoardSize, configuration.SimulatedAnnealingInitialBoard);
        var steps = new List<NQueensStep>
        {
            CreateStep(1, NQueensAlgorithmKind.SimulatedAnnealing, NQueensStepKind.Initial, current, null, null, 18.0, Fitness(current), true, false)
        };

        var random = new Random(73);
        const double initialTemperature = 18.0;
        const double coolingFactor = 0.92;
        const double minimumTemperature = 0.2;
        const int maxIterations = 30;

        for (var iteration = 0; iteration < maxIterations; iteration++)
        {
            var currentConflicts = CountConflicts(current);
            if (currentConflicts == 0)
            {
                steps.Add(CreateStep(steps.Count + 1, NQueensAlgorithmKind.SimulatedAnnealing, NQueensStepKind.Solved, current, null, null, TemperatureAt(iteration, initialTemperature, coolingFactor), Fitness(current), true, true));
                return new NQueensAlgorithmDemo("SimulatedAnnealing", "Simulated annealing", "Can accept worse moves while the temperature is still high enough to escape local optima.", configuration.BoardSize, CreateBoard(configuration.BoardSize, configuration.SimulatedAnnealingInitialBoard), steps);
            }

            var temperature = TemperatureAt(iteration, initialTemperature, coolingFactor);
            if (temperature < minimumTemperature)
            {
                break;
            }

            var candidateColumn = random.Next(configuration.BoardSize);
            var candidateRow = random.Next(configuration.BoardSize - 1);
            if (candidateRow >= current[candidateColumn])
            {
                candidateRow++;
            }

            var candidate = CreateBoard(configuration.BoardSize, current);
            candidate[candidateColumn] = candidateRow;

            var candidateConflicts = CountConflicts(candidate);
            var accepted = candidateConflicts <= currentConflicts
                || Math.Exp((currentConflicts - candidateConflicts) / temperature) > random.NextDouble();

            if (accepted)
            {
                current = candidate;
                steps.Add(CreateStep(steps.Count + 1, NQueensAlgorithmKind.SimulatedAnnealing, NQueensStepKind.Move, current, candidateColumn, candidateRow, temperature, Fitness(current), true, CountConflicts(current) == 0));
            }
            else
            {
                steps.Add(CreateStep(steps.Count + 1, NQueensAlgorithmKind.SimulatedAnnealing, NQueensStepKind.RejectedMove, current, candidateColumn, candidateRow, temperature, Fitness(current), false, false));
            }
        }

        var solved = CountConflicts(current) == 0;
        steps.Add(CreateStep(steps.Count + 1, NQueensAlgorithmKind.SimulatedAnnealing, solved ? NQueensStepKind.Solved : NQueensStepKind.Stopped, current, null, null, minimumTemperature, Fitness(current), true, solved));
        return new NQueensAlgorithmDemo("SimulatedAnnealing", "Simulated annealing", "Can accept worse moves while the temperature is still high enough to escape local optima.", configuration.BoardSize, CreateBoard(configuration.BoardSize, configuration.SimulatedAnnealingInitialBoard), steps);
    }

    private static NQueensAlgorithmDemo SimulateGeneticAlgorithm(NQueensDemoConfiguration configuration)
    {
        const int populationSize = 24;
        const int maxGenerations = 30;
        const double mutationRate = 0.14;
        var random = new Random(configuration.GeneticSeed);
        var population = Enumerable.Range(0, populationSize)
            .Select(_ => Enumerable.Range(0, configuration.BoardSize).Select(__ => random.Next(configuration.BoardSize)).ToArray())
            .ToList();

        var initialBest = population.OrderBy(CountConflicts).First();
        var steps = new List<NQueensStep>
        {
            CreateStep(1, NQueensAlgorithmKind.GeneticAlgorithm, NQueensStepKind.Initial, initialBest, null, 0, null, Fitness(initialBest), true, CountConflicts(initialBest) == 0)
        };

        for (var generation = 1; generation <= maxGenerations; generation++)
        {
            var best = population.OrderBy(CountConflicts).First();
            var bestConflicts = CountConflicts(best);
            var solved = bestConflicts == 0;

            steps.Add(CreateStep(steps.Count + 1, NQueensAlgorithmKind.GeneticAlgorithm, solved ? NQueensStepKind.Solved : NQueensStepKind.Generation, best, null, generation, null, Fitness(best), true, solved));
            if (solved)
            {
                return new NQueensAlgorithmDemo("GeneticAlgorithm", "Genetic algorithm", "Evolves a population of boards through selection, crossover and mutation.", configuration.BoardSize, CreateBoard(configuration.BoardSize, initialBest), steps);
            }

            var nextPopulation = new List<int[]> { CreateBoard(configuration.BoardSize, best) };
            while (nextPopulation.Count < populationSize)
            {
                var parentA = TournamentSelect(population, random);
                var parentB = TournamentSelect(population, random);
                var child = Crossover(parentA, parentB, random);
                Mutate(child, configuration.BoardSize, mutationRate, random);
                nextPopulation.Add(child);
            }

            population = nextPopulation;
        }

        var finalBest = population.OrderBy(CountConflicts).First();
        steps.Add(CreateStep(steps.Count + 1, NQueensAlgorithmKind.GeneticAlgorithm, NQueensStepKind.Stopped, finalBest, null, maxGenerations + 1, null, Fitness(finalBest), true, false));
        return new NQueensAlgorithmDemo("GeneticAlgorithm", "Genetic algorithm", "Evolves a population of boards through selection, crossover and mutation.", configuration.BoardSize, CreateBoard(configuration.BoardSize, initialBest), steps);
    }

    private static NQueensDemoConfiguration CreateDefaultConfiguration(int boardSize)
    {
        return new NQueensDemoConfiguration(
            boardSize,
            CreateBoard(boardSize, HillClimbingInitialBoard),
            CreateBoard(boardSize, AnnealingInitialBoard),
            113);
    }

    private static int[] CreateInterestingRandomBoard(int boardSize, Random random)
    {
        for (var attempt = 0; attempt < 16; attempt++)
        {
            var board = Enumerable.Range(0, boardSize)
                .Select(_ => random.Next(boardSize))
                .ToArray();

            if (CountConflicts(board) > 0)
            {
                return board;
            }
        }

        var fallback = Enumerable.Range(0, boardSize)
            .Select(index => index % boardSize)
            .ToArray();
        fallback[0] = (fallback[0] + 1) % boardSize;
        return fallback;
    }

    private static NQueensStep CreateStep(
        int stepNumber,
        NQueensAlgorithmKind algorithm,
        NQueensStepKind kind,
        int[] board,
        int? currentColumn,
        int? currentRowOrGeneration,
        double? temperature,
        double? fitness,
        bool acceptedMove,
        bool solved)
    {
        int? currentRow = currentColumn.HasValue ? currentRowOrGeneration : null;
        int? generation = currentColumn.HasValue ? null : currentRowOrGeneration;
        var queenRows = CreateBoard(board.Length, board);
        var assignment = FormatAssignment(queenRows);
        var (conflicts, conflictedColumns) = EvaluateBoard(queenRows);

        var metric = algorithm switch
        {
            NQueensAlgorithmKind.HillClimbing => $"fitness={FormatDouble(fitness ?? 0)}",
            NQueensAlgorithmKind.SimulatedAnnealing => $"T={FormatDouble(temperature ?? 0)}, fitness={FormatDouble(fitness ?? 0)}",
            _ => $"generation={generation ?? 0}, fitness={FormatDouble(fitness ?? 0)}"
        };

        var summary = kind switch
        {
            NQueensStepKind.Initial => "Initial board",
            NQueensStepKind.Move when currentColumn.HasValue && currentRow.HasValue => $"Move Q{currentColumn.Value + 1} to row {currentRow.Value + 1}",
            NQueensStepKind.RejectedMove when currentColumn.HasValue && currentRow.HasValue => $"Reject Q{currentColumn.Value + 1} -> row {currentRow.Value + 1}",
            NQueensStepKind.Generation => $"Generation {generation}",
            NQueensStepKind.Solved => "Solution found",
            _ => "Stop condition reached"
        };

        var description = kind switch
        {
            NQueensStepKind.Initial => $"Start with assignment {assignment}.",
            NQueensStepKind.Move => $"The new state is {assignment} with {conflicts} conflicting queen pairs.",
            NQueensStepKind.RejectedMove => $"The candidate move is rejected, so the board stays at {assignment}.",
            NQueensStepKind.Generation => $"This generation keeps the currently best board {assignment} with {conflicts} conflicts.",
            NQueensStepKind.Solved => $"All queens are placed without conflicts: {assignment}.",
            _ => $"The run stops at {assignment} with {conflicts} remaining conflicts."
        };

        return new NQueensStep(
            stepNumber,
            summary,
            metric,
            description,
            queenRows,
            currentColumn,
            currentRow,
            conflictedColumns,
            conflicts,
            solved,
            kind == NQueensStepKind.Stopped && !solved);
    }

    private static HillClimbingMove? FindBestHillClimbingMove(int[] board)
    {
        HillClimbingMove? bestMove = null;
        for (var column = 0; column < board.Length; column++)
        {
            for (var row = 0; row < board.Length; row++)
            {
                if (row == board[column])
                {
                    continue;
                }

                var candidate = CreateBoard(board.Length, board);
                candidate[column] = row;
                var conflicts = CountConflicts(candidate);
                if (bestMove is null
                    || conflicts < bestMove.ConflictCount
                    || (conflicts == bestMove.ConflictCount && (column < bestMove.Column || (column == bestMove.Column && row < bestMove.Row))))
                {
                    bestMove = new HillClimbingMove(column, row, conflicts);
                }
            }
        }

        return bestMove;
    }

    private static int[] TournamentSelect(IReadOnlyList<int[]> population, Random random)
    {
        return Enumerable.Range(0, 3)
            .Select(_ => population[random.Next(population.Count)])
            .OrderBy(CountConflicts)
            .First();
    }

    private static int[] Crossover(int[] parentA, int[] parentB, Random random)
    {
        var crossoverPoint = random.Next(1, parentA.Length - 1);
        var child = new int[parentA.Length];
        for (var index = 0; index < parentA.Length; index++)
        {
            child[index] = index < crossoverPoint ? parentA[index] : parentB[index];
        }

        return child;
    }

    private static void Mutate(int[] board, int boardSize, double mutationRate, Random random)
    {
        for (var column = 0; column < board.Length; column++)
        {
            if (random.NextDouble() < mutationRate)
            {
                board[column] = random.Next(boardSize);
            }
        }
    }

    private static int[] CreateBoard(int boardSize, IReadOnlyList<int> source)
    {
        var board = new int[boardSize];
        for (var index = 0; index < boardSize; index++)
        {
            board[index] = source[index];
        }

        return board;
    }

    private static (int ConflictCount, IReadOnlyList<int> ConflictedColumns) EvaluateBoard(IReadOnlyList<int> board)
    {
        var csp = CreateCsp(board.Count);
        var assignment = CreateAssignment(csp, board);
        var conflictCount = csp.Constraints.Count(constraint => !constraint.IsSatisfiedWith(assignment));
        var conflictedColumns = csp.Variables
            .Select((variable, index) => new { variable, index })
            .Where(entry => csp.GetConstraints(entry.variable).Any(constraint => !constraint.IsSatisfiedWith(assignment)))
            .Select(entry => entry.index)
            .ToArray();

        return (conflictCount, conflictedColumns);
    }

    private static int CountConflicts(IReadOnlyList<int> board)
    {
        return EvaluateBoard(board).ConflictCount;
    }

    private static double Fitness(IReadOnlyList<int> board)
    {
        var maxPairs = (board.Count * (board.Count - 1)) / 2.0;
        return (maxPairs - CountConflicts(board)) / maxPairs;
    }

    private static double TemperatureAt(int iteration, double initialTemperature, double coolingFactor)
    {
        return initialTemperature * Math.Pow(coolingFactor, iteration);
    }

    private static string FormatAssignment(IReadOnlyList<int> queenRows)
    {
        return string.Join(", ", queenRows.Select((row, column) => $"Q{column + 1}->{row + 1}"));
    }

    private static string BuildPackageReferenceSummary(int boardSize)
    {
        var csp = CreateCsp(boardSize);
        var solution = new MinConflictsSolver<Variable, int>(2000).Solve(csp);
        if (solution is null)
        {
            return "no solution found within 2000 MinConflicts steps";
        }

        var board = csp.Variables.Select(variable => solution.GetValue(variable)).ToArray();
        return $"MinConflictsSolver => {FormatAssignment(board)}";
    }

    private static CSP<Variable, int> CreateCsp(int boardSize)
    {
        var variables = Enumerable.Range(1, boardSize).Select(index => new Variable($"Q{index}")).ToList();
        var csp = new CSP<Variable, int>(variables);
        var domain = new Domain<int>(Enumerable.Range(0, boardSize));
        foreach (var variable in variables)
        {
            csp.SetDomain(variable, domain);
        }

        for (var left = 0; left < variables.Count; left++)
        {
            for (var right = left + 1; right < variables.Count; right++)
            {
                csp.AddConstraint(new QueenConstraint(variables[left], variables[right], left, right));
            }
        }

        return csp;
    }

    private static Assignment<Variable, int> CreateAssignment(CSP<Variable, int> csp, IReadOnlyList<int> board)
    {
        var assignment = new Assignment<Variable, int>();
        for (var index = 0; index < csp.Variables.Count; index++)
        {
            assignment.Add(csp.Variables[index], board[index]);
        }

        return assignment;
    }

    private static string FormatDouble(double value) => value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);

    private sealed class QueenConstraint : IConstraint<Variable, int>
    {
        private readonly Variable _leftVariable;
        private readonly Variable _rightVariable;
        private readonly int _leftColumn;
        private readonly int _rightColumn;

        public QueenConstraint(Variable leftVariable, Variable rightVariable, int leftColumn, int rightColumn)
        {
            _leftVariable = leftVariable;
            _rightVariable = rightVariable;
            _leftColumn = leftColumn;
            _rightColumn = rightColumn;
            Scope = [_leftVariable, _rightVariable];
        }

        public IList<Variable> Scope { get; }

        public bool IsSatisfiedWith(IAssignment<Variable, int> assignment)
        {
            if (!assignment.Contains(_leftVariable) || !assignment.Contains(_rightVariable))
            {
                return true;
            }

            var leftRow = assignment.GetValue(_leftVariable);
            var rightRow = assignment.GetValue(_rightVariable);
            return leftRow != rightRow && Math.Abs(leftRow - rightRow) != Math.Abs(_leftColumn - _rightColumn);
        }
    }

    private sealed record HillClimbingMove(int Column, int Row, int ConflictCount);
}