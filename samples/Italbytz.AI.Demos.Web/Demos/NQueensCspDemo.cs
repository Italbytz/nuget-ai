using Italbytz.AI.CSP;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record NQueensCspStep(
    int Number,
    string Summary,
    string Description,
    string Metric,
    int[] QueenRows,
    IReadOnlyList<int> AssignedColumns,
    IReadOnlyList<int> ConflictedColumns,
    int ConflictCount,
    int? CurrentColumn,
    bool Solved,
    bool Backtracking);

internal sealed record NQueensCspAlgorithmDemo(
    string Key,
    string Name,
    string Summary,
    string SeedSummary,
    int BoardSize,
    int[] InitialQueenRows,
    IReadOnlyList<int> InitialAssignedColumns,
    IReadOnlyList<NQueensCspStep> Steps);

internal static class NQueensCspDemoFactory
{
    private const int BoardSize = 8;

    public static IReadOnlyList<NQueensCspAlgorithmDemo> BuildAll(int seed)
    {
        return
        [
            SimulateBacktracking(seed, useArcConsistency: false),
            SimulateBacktracking(seed + 17, useArcConsistency: true),
            SimulateMinConflicts(seed + 31)
        ];
    }

    public static int CountConflictsForBoard(IReadOnlyList<int> board, IReadOnlyList<int>? assignedColumns = null)
    {
        var assigned = NormalizeAssignedColumns(board.Count, assignedColumns);
        var conflicts = 0;
        for (var left = 0; left < board.Count; left++)
        {
            if (!assigned.Contains(left))
            {
                continue;
            }

            for (var right = left + 1; right < board.Count; right++)
            {
                if (!assigned.Contains(right))
                {
                    continue;
                }

                if (IsConflict(left, board[left], right, board[right]))
                {
                    conflicts++;
                }
            }
        }

        return conflicts;
    }

    private static NQueensCspAlgorithmDemo SimulateBacktracking(int seed, bool useArcConsistency)
    {
        var random = new Random(seed);
        var initialBoard = Enumerable.Range(0, BoardSize)
            .Select(_ => random.Next(BoardSize))
            .ToArray();
        var board = initialBoard.ToArray();
        var assignment = new Dictionary<int, int>();
        var rowOrder = Enumerable.Range(0, BoardSize)
            .OrderBy(_ => random.Next())
            .ToArray();

        var steps = new List<NQueensCspStep>();
        var stepNumber = 1;

        steps.Add(CreateStep(
            stepNumber++,
            "Initial board",
            useArcConsistency
                ? "Start with an empty assignment and use arc consistency after every committed queen move."
                : "Start with an empty assignment and assign queens column by column with plain backtracking.",
            board,
            assignment.Keys,
            currentColumn: null,
            solved: false,
            backtracking: false,
            extraMetric: useArcConsistency ? "AC-3=ready" : "AC-3=off"));

        var domains = CreateInitialDomains();
        var solved = Search(domains);
        if (!solved)
        {
            steps.Add(CreateStep(
                stepNumber,
                "Stop after explored trace",
                "The trace budget is exhausted without a complete assignment. Randomize the run to inspect a different branch order.",
                board,
                assignment.Keys,
                currentColumn: null,
                solved: false,
                backtracking: false,
                extraMetric: useArcConsistency ? "AC-3=active" : "AC-3=off"));
        }

        return new NQueensCspAlgorithmDemo(
            useArcConsistency ? "ArcConsistency" : "Backtracking",
            useArcConsistency ? "Backtracking + arc consistency" : "Backtracking",
            useArcConsistency
                ? "Backtracking with AC-3 look-ahead prunes unsupported row values after each assignment."
                : "Classic depth-first backtracking explores assignments and undoes choices when a conflict appears.",
            $"Run seed {seed}",
            BoardSize,
            initialBoard,
            [],
            steps);

        bool Search(List<HashSet<int>> currentDomains)
        {
            if (assignment.Count == BoardSize)
            {
                steps.Add(CreateStep(
                    stepNumber++,
                    "Solution found",
                    "All queens are assigned and no pair attacks each other.",
                    board,
                    assignment.Keys,
                    currentColumn: null,
                    solved: true,
                    backtracking: false,
                    extraMetric: useArcConsistency ? "AC-3=active" : "AC-3=off"));
                return true;
            }

            var column = SelectNextColumn(currentDomains, assignment.Keys, useArcConsistency);
            var candidateRows = OrderedRowsForColumn(currentDomains[column], rowOrder);

            foreach (var row in candidateRows)
            {
                var previousRow = board[column];
                board[column] = row;
                assignment[column] = row;

                var locallyConsistent = IsConsistent(column, assignment);
                if (!locallyConsistent)
                {
                    steps.Add(CreateStep(
                        stepNumber++,
                        $"Reject Q{column + 1} -> row {row + 1}",
                        "The assignment directly violates a row or diagonal constraint with an already assigned queen.",
                        board,
                        assignment.Keys,
                        currentColumn: column,
                        solved: false,
                        backtracking: false,
                        extraMetric: useArcConsistency ? "AC-3=skip" : "AC-3=off"));

                    assignment.Remove(column);
                    board[column] = previousRow;
                    continue;
                }

                var nextDomains = CloneDomains(currentDomains);
                nextDomains[column].Clear();
                nextDomains[column].Add(row);

                var (arcConsistent, pruned) = useArcConsistency
                    ? EnforceArcConsistency(nextDomains)
                    : (true, 0);

                steps.Add(CreateStep(
                    stepNumber++,
                    $"Assign Q{column + 1} -> row {row + 1}",
                    useArcConsistency
                        ? "Commit the assignment and propagate constraints with AC-3 before descending further."
                        : "Commit the assignment and continue with the next unassigned column.",
                    board,
                    assignment.Keys,
                    currentColumn: column,
                    solved: false,
                    backtracking: false,
                    extraMetric: useArcConsistency ? $"AC-3 pruned={pruned}" : "AC-3=off"));

                if (arcConsistent && Search(nextDomains))
                {
                    return true;
                }

                assignment.Remove(column);
                board[column] = previousRow;

                steps.Add(CreateStep(
                    stepNumber++,
                    $"Backtrack from Q{column + 1}",
                    useArcConsistency
                        ? "A deeper branch became inconsistent after propagation, so the solver reverts this queen placement."
                        : "A deeper branch failed, so this queen placement is undone and the next row is tested.",
                    board,
                    assignment.Keys,
                    currentColumn: column,
                    solved: false,
                    backtracking: true,
                    extraMetric: useArcConsistency ? "AC-3=active" : "AC-3=off"));
            }

            return false;
        }
    }

    private static NQueensCspAlgorithmDemo SimulateMinConflicts(int seed)
    {
        const int maxIterations = 36;

        var random = new Random(seed);
        var board = Enumerable.Range(0, BoardSize)
            .Select(_ => random.Next(BoardSize))
            .ToArray();
        var initialBoard = board.ToArray();
        var allColumns = Enumerable.Range(0, BoardSize).ToArray();
        var steps = new List<NQueensCspStep>();
        var stepNumber = 1;

        steps.Add(CreateStep(
            stepNumber++,
            "Initial complete assignment",
            "Min-conflicts starts from a full but typically inconsistent assignment and repairs one conflicted queen at a time.",
            board,
            allColumns,
            currentColumn: null,
            solved: CountConflictsForBoard(board, allColumns) == 0,
            backtracking: false,
            extraMetric: "repair=0"));

        for (var iteration = 1; iteration <= maxIterations; iteration++)
        {
            var conflictedColumns = GetConflictedColumns(board, allColumns);
            if (conflictedColumns.Count == 0)
            {
                steps.Add(CreateStep(
                    stepNumber++,
                    "Solution found",
                    "No queen is in conflict, so the repair process converged to a valid solution.",
                    board,
                    allColumns,
                    currentColumn: null,
                    solved: true,
                    backtracking: false,
                    extraMetric: $"repair={iteration - 1}"));
                return new NQueensCspAlgorithmDemo(
                    "MinConflicts",
                    "Min-conflicts",
                    "A complete assignment is repaired iteratively by relocating a conflicted queen to the least-conflicting row.",
                    $"Run seed {seed}",
                    BoardSize,
                    initialBoard,
                    allColumns,
                    steps);
            }

            var column = conflictedColumns[random.Next(conflictedColumns.Count)];
            var bestRows = Enumerable.Range(0, BoardSize)
                .Select(row => new
                {
                    Row = row,
                    Conflicts = CountConflictsIfMoved(board, column, row)
                })
                .ToArray();

            var minConflicts = bestRows.Min(entry => entry.Conflicts);
            var bestCandidates = bestRows
                .Where(entry => entry.Conflicts == minConflicts)
                .Select(entry => entry.Row)
                .ToArray();
            var selectedRow = bestCandidates[random.Next(bestCandidates.Length)];
            board[column] = selectedRow;

            steps.Add(CreateStep(
                stepNumber++,
                $"Repair Q{column + 1} -> row {selectedRow + 1}",
                "Pick a conflicted queen and move it to a row with minimal resulting conflicts in the current full assignment.",
                board,
                allColumns,
                currentColumn: column,
                solved: false,
                backtracking: false,
                extraMetric: $"repair={iteration}"));
        }

        steps.Add(CreateStep(
            stepNumber,
            "Stop after repair budget",
            "The configured number of repair steps is exhausted. Randomize the run to inspect another trajectory.",
            board,
            allColumns,
            currentColumn: null,
            solved: false,
            backtracking: false,
            extraMetric: $"repair={maxIterations}"));

        return new NQueensCspAlgorithmDemo(
            "MinConflicts",
            "Min-conflicts",
            "A complete assignment is repaired iteratively by relocating a conflicted queen to the least-conflicting row.",
            $"Run seed {seed}",
            BoardSize,
            initialBoard,
            allColumns,
            steps);
    }

    private static NQueensCspStep CreateStep(
        int number,
        string summary,
        string description,
        int[] board,
        IEnumerable<int> assignedColumns,
        int? currentColumn,
        bool solved,
        bool backtracking,
        string extraMetric)
    {
        var assigned = assignedColumns
            .Distinct()
            .OrderBy(column => column)
            .ToArray();
        var conflictCount = CountConflictsForBoard(board, assigned);
        var conflictedColumns = GetConflictedColumns(board, assigned);

        return new NQueensCspStep(
            number,
            summary,
            description,
            $"assigned={assigned.Length}/{board.Length}, conflicts={conflictCount}, {extraMetric}",
            board.ToArray(),
            assigned,
            conflictedColumns,
            conflictCount,
            currentColumn,
            solved,
            backtracking);
    }

    private static List<HashSet<int>> CreateInitialDomains()
    {
        return Enumerable.Range(0, BoardSize)
            .Select(_ => new HashSet<int>(Enumerable.Range(0, BoardSize)))
            .ToList();
    }

    private static List<HashSet<int>> CloneDomains(IReadOnlyList<HashSet<int>> domains)
    {
        return domains
            .Select(domain => new HashSet<int>(domain))
            .ToList();
    }

    private static int SelectNextColumn(IReadOnlyList<HashSet<int>> domains, IEnumerable<int> assignedColumns, bool useMrv)
    {
        var assigned = new HashSet<int>(assignedColumns);
        var unassigned = Enumerable.Range(0, domains.Count)
            .Where(column => !assigned.Contains(column))
            .ToArray();

        if (!useMrv)
        {
            return unassigned[0];
        }

        var bestSize = int.MaxValue;
        var bestColumn = unassigned[0];
        foreach (var column in unassigned)
        {
            var size = domains[column].Count;
            if (size < bestSize)
            {
                bestSize = size;
                bestColumn = column;
            }
        }

        return bestColumn;
    }

    private static IReadOnlyList<int> OrderedRowsForColumn(HashSet<int> domain, IReadOnlyList<int> rowOrder)
    {
        return rowOrder.Where(domain.Contains).ToArray();
    }

    private static bool IsConsistent(int newColumn, IReadOnlyDictionary<int, int> assignment)
    {
        var newRow = assignment[newColumn];
        foreach (var (otherColumn, otherRow) in assignment)
        {
            if (otherColumn == newColumn)
            {
                continue;
            }

            if (IsConflict(newColumn, newRow, otherColumn, otherRow))
            {
                return false;
            }
        }

        return true;
    }

    private static (bool Consistent, int PrunedValues) EnforceArcConsistency(List<HashSet<int>> domains)
    {
        var queue = new Queue<(int Xi, int Xj)>();
        for (var i = 0; i < domains.Count; i++)
        {
            for (var j = 0; j < domains.Count; j++)
            {
                if (i != j)
                {
                    queue.Enqueue((i, j));
                }
            }
        }

        var prunedValues = 0;
        while (queue.Count > 0)
        {
            var (xi, xj) = queue.Dequeue();
            if (!Revise(domains, xi, xj, out var removedCount))
            {
                continue;
            }

            prunedValues += removedCount;
            if (domains[xi].Count == 0)
            {
                return (false, prunedValues);
            }

            for (var xk = 0; xk < domains.Count; xk++)
            {
                if (xk != xi && xk != xj)
                {
                    queue.Enqueue((xk, xi));
                }
            }
        }

        return (true, prunedValues);
    }

    private static bool Revise(List<HashSet<int>> domains, int xi, int xj, out int removedCount)
    {
        removedCount = 0;
        var toRemove = new List<int>();

        foreach (var valueXi in domains[xi])
        {
            var supported = domains[xj].Any(valueXj => !IsConflict(xi, valueXi, xj, valueXj));
            if (!supported)
            {
                toRemove.Add(valueXi);
            }
        }

        foreach (var value in toRemove)
        {
            domains[xi].Remove(value);
        }

        removedCount = toRemove.Count;
        return removedCount > 0;
    }

    private static int CountConflictsIfMoved(IReadOnlyList<int> board, int column, int row)
    {
        var candidate = board.ToArray();
        candidate[column] = row;
        return CountConflictsForBoard(candidate);
    }

    private static IReadOnlyList<int> GetConflictedColumns(IReadOnlyList<int> board, IReadOnlyList<int>? assignedColumns)
    {
        var assigned = NormalizeAssignedColumns(board.Count, assignedColumns);
        return assigned
            .Where(column => assigned.Any(other => other != column && IsConflict(column, board[column], other, board[other])))
            .ToArray();
    }

    private static IReadOnlyList<int> NormalizeAssignedColumns(int boardSize, IReadOnlyList<int>? assignedColumns)
    {
        if (assignedColumns is null || assignedColumns.Count == 0)
        {
            return Enumerable.Range(0, boardSize).ToArray();
        }

        return assignedColumns
            .Distinct()
            .OrderBy(column => column)
            .ToArray();
    }

    private static bool IsConflict(int leftColumn, int leftRow, int rightColumn, int rightRow)
    {
        return leftRow == rightRow || Math.Abs(leftRow - rightRow) == Math.Abs(leftColumn - rightColumn);
    }
}
