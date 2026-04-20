using Italbytz.AI.CSP;
using Italbytz.AI.CSP.Examples;
using Italbytz.AI.CSP.Solver;

namespace Italbytz.AI.Demos.Web.Demos;

internal enum MapColoringAlgorithmKind
{
    StandardBacktracking,
    HeuristicBacktracking,
    MinConflicts
}

internal sealed record MapColoringStep(
    int Number,
    string Summary,
    string Description,
    string Metric,
    string? CurrentRegion,
    string? CurrentColor,
    IReadOnlyDictionary<string, string> Assignments,
    IReadOnlyList<string> ConflictedRegions,
    int ConflictCount,
    bool Solved,
    bool Backtracking);

internal sealed record MapColoringAlgorithmDemo(
    string Key,
    string Name,
    string Summary,
    string SeedSummary,
    IReadOnlyList<string> RegionOrder,
    IReadOnlyList<string> ColorOrder,
    IReadOnlyList<MapColoringStep> Steps);

internal static class MapColoringDemoFactory
{
    public static IReadOnlyList<MapColoringAlgorithmDemo> BuildAll(int seed)
    {
        return
        [
            SimulateBacktracking(seed, MapColoringAlgorithmKind.StandardBacktracking),
            SimulateBacktracking(seed + 17, MapColoringAlgorithmKind.HeuristicBacktracking),
            SimulateMinConflicts(seed + 31)
        ];
    }

    private static MapColoringAlgorithmDemo SimulateBacktracking(int seed, MapColoringAlgorithmKind kind)
    {
        var csp = new MapCSP();
        var random = new Random(seed);
        var regionOrder = Shuffle(csp.Variables.Select(variable => variable.Name), random);
        var colorOrder = Shuffle(MapCSP.Colors, random);
        var steps = new List<MapColoringStep>();
        var assignment = new Assignment<Variable, string>();
        var variableStrategy = kind == MapColoringAlgorithmKind.HeuristicBacktracking
            ? CspHeuristics.MrvDeg<Variable, string>()
            : null;
        var valueStrategy = kind == MapColoringAlgorithmKind.HeuristicBacktracking
            ? CspHeuristics.Lcv<Variable, string>()
            : null;
        var stepNumber = 1;

        Search(assignment);

        return new MapColoringAlgorithmDemo(
            kind.ToString(),
            kind == MapColoringAlgorithmKind.StandardBacktracking ? "Backtracking" : "Heuristic backtracking",
            kind == MapColoringAlgorithmKind.StandardBacktracking
                ? "Classic backtracking assigns one region after the other and undoes choices whenever a constraint is violated deeper in the search tree."
                : "Backtracking with MRV/DEG variable selection and LCV value ordering keeps the search tree smaller on the same map coloring problem.",
            $"Run seed {seed}",
            regionOrder,
            colorOrder,
            steps);

        bool Search(Assignment<Variable, string> current)
        {
            if (current.IsSolution(csp))
            {
                steps.Add(CreateStep(
                    stepNumber++,
                    "Solution found",
                    "All regions are assigned without violating any neighboring color constraint.",
                    current,
                    null,
                    null,
                    solved: true,
                    backtracking: false));
                return true;
            }

            var unassigned = csp.Variables.Where(variable => !current.Contains(variable)).ToList();
            var variable = SelectVariable(csp, current, unassigned, regionOrder, variableStrategy);
            var values = OrderValues(csp, current, variable, colorOrder, valueStrategy);

            foreach (var value in values)
            {
                current.Add(variable, value);
                var consistent = current.IsConsistent(csp.GetConstraints(variable));
                var selectionHint = kind == MapColoringAlgorithmKind.HeuristicBacktracking
                    ? " using MRV/DEG and LCV"
                    : string.Empty;

                steps.Add(CreateStep(
                    stepNumber++,
                    $"Try {variable.Name} = {FormatColorLabel(value)}",
                    consistent
                        ? $"Assign {variable.Name} to {FormatColorLabel(value)}{selectionHint} and continue with the remaining regions."
                        : $"Assigning {variable.Name} to {FormatColorLabel(value)} immediately breaks a neighboring constraint and is rejected.",
                    current,
                    variable.Name,
                    value,
                    solved: false,
                    backtracking: false));

                if (consistent && Search(current))
                {
                    return true;
                }

                current.Remove(variable);

                if (consistent)
                {
                    steps.Add(CreateStep(
                        stepNumber++,
                        $"Backtrack from {variable.Name}",
                        $"The deeper branch below {variable.Name} = {FormatColorLabel(value)} failed, so the solver removes that assignment and tries the next color.",
                        current,
                        variable.Name,
                        value,
                        solved: false,
                        backtracking: true));
                }
            }

            return false;
        }
    }

    private static MapColoringAlgorithmDemo SimulateMinConflicts(int seed)
    {
        const int maxSteps = 24;

        var csp = new MapCSP();
        var random = new Random(seed);
        var regionOrder = Shuffle(csp.Variables.Select(variable => variable.Name), random);
        var colorOrder = Shuffle(MapCSP.Colors, random);
        var steps = new List<MapColoringStep>();
        var assignment = new Assignment<Variable, string>();
        var stepNumber = 1;

        foreach (var variable in csp.Variables.OrderBy(variable => RegionIndex(variable.Name, regionOrder)))
        {
            var value = colorOrder[random.Next(colorOrder.Count)];
            assignment.Add(variable, value);
        }

        steps.Add(CreateStep(
            stepNumber++,
            "Initial random assignment",
            "Min-conflicts starts with a complete but usually inconsistent coloring and then repairs one conflicted region at a time.",
            assignment,
            null,
            null,
            solved: assignment.IsSolution(csp),
            backtracking: false));

        for (var iteration = 0; iteration < maxSteps && !assignment.IsSolution(csp); iteration++)
        {
            var conflicted = GetConflictedRegions(csp, assignment)
                .OrderBy(region => RegionIndex(region, regionOrder))
                .ToArray();
            if (conflicted.Length == 0)
            {
                break;
            }

            var selectedRegion = conflicted[random.Next(conflicted.Length)];
            var variable = csp.Variables.First(candidate => candidate.Name == selectedRegion);
            var candidateScores = colorOrder
                .Select(color => new
                {
                    Color = color,
                    Conflicts = CountConflictsForValue(csp, assignment, variable, color)
                })
                .ToArray();
            var bestConflictCount = candidateScores.Min(entry => entry.Conflicts);
            var bestCandidates = candidateScores
                .Where(entry => entry.Conflicts == bestConflictCount)
                .Select(entry => entry.Color)
                .ToArray();
            var bestColor = bestCandidates[random.Next(bestCandidates.Length)];
            assignment.Add(variable, bestColor);

            steps.Add(CreateStep(
                stepNumber++,
                $"Repair {selectedRegion} -> {FormatColorLabel(bestColor)}",
                $"Pick a conflicted region and switch it to the color that minimizes the number of violated neighbor constraints in the current full assignment.",
                assignment,
                selectedRegion,
                bestColor,
                solved: assignment.IsSolution(csp),
                backtracking: false));
        }

        if (assignment.IsSolution(csp))
        {
            steps.Add(CreateStep(
                stepNumber++,
                "Solution found",
                "The repair loop converged to a conflict-free coloring.",
                assignment,
                null,
                null,
                solved: true,
                backtracking: false));
        }
        else
        {
            steps.Add(CreateStep(
                stepNumber++,
                "Stop after repair budget",
                "The configured repair budget is exhausted. Randomize the run to inspect a different repair trajectory.",
                assignment,
                null,
                null,
                solved: false,
                backtracking: false));
        }

        return new MapColoringAlgorithmDemo(
            MapColoringAlgorithmKind.MinConflicts.ToString(),
            "Min-conflicts",
            "A complete assignment is repaired iteratively by recoloring conflicted regions instead of exploring a search tree.",
            $"Run seed {seed}",
            regionOrder,
            colorOrder,
            steps);
    }

    private static Variable SelectVariable(
        MapCSP csp,
        Assignment<Variable, string> assignment,
        IList<Variable> unassigned,
        IReadOnlyList<string> regionOrder,
        CspHeuristics.IVariableSelectionStrategy<Variable, string>? variableStrategy)
    {
        if (variableStrategy is null)
        {
            return unassigned.OrderBy(variable => RegionIndex(variable.Name, regionOrder)).First();
        }

        return variableStrategy
            .Apply(csp, unassigned)
            .OrderBy(variable => RegionIndex(variable.Name, regionOrder))
            .First();
    }

    private static IReadOnlyList<string> OrderValues(
        MapCSP csp,
        Assignment<Variable, string> assignment,
        Variable variable,
        IReadOnlyList<string> colorOrder,
        CspHeuristics.IValueOrderingStrategy<Variable, string>? valueStrategy)
    {
        var ordered = valueStrategy is null
            ? csp.GetDomain(variable).ToList()
            : valueStrategy.Apply(csp, assignment, variable).ToList();

        return ordered
            .OrderBy(value => ordered.IndexOf(value))
            .ThenBy(value => ColorIndex(value, colorOrder))
            .ToArray();
    }

    private static MapColoringStep CreateStep(
        int number,
        string summary,
        string description,
        Assignment<Variable, string> assignment,
        string? currentRegion,
        string? currentColor,
        bool solved,
        bool backtracking)
    {
        var csp = new MapCSP();
        var assignments = csp.Variables
            .Where(assignment.Contains)
            .ToDictionary(variable => variable.Name, variable => assignment.GetValue(variable) ?? string.Empty, StringComparer.Ordinal);
        var conflictedRegions = GetConflictedRegions(csp, assignment);
        var conflictCount = csp.Constraints.Count(constraint => !constraint.IsSatisfiedWith(assignment));

        return new MapColoringStep(
            number,
            summary,
            description,
            $"assigned={assignments.Count}/7, conflicts={conflictCount}",
            currentRegion,
            currentColor,
            assignments,
            conflictedRegions,
            conflictCount,
            solved,
            backtracking);
    }

    private static IReadOnlyList<string> GetConflictedRegions(MapCSP csp, Assignment<Variable, string> assignment)
    {
        return csp.Variables
            .Where(variable => assignment.Contains(variable) && csp.GetConstraints(variable).Any(constraint => !constraint.IsSatisfiedWith(assignment)))
            .Select(variable => variable.Name)
            .ToArray();
    }

    private static int CountConflictsForValue(MapCSP csp, Assignment<Variable, string> assignment, Variable variable, string color)
    {
        var candidate = (Assignment<Variable, string>)assignment.Clone();
        candidate.Add(variable, color);
        return csp.GetConstraints(variable).Count(constraint => !constraint.IsSatisfiedWith(candidate));
    }

    private static List<string> Shuffle(IEnumerable<string> values, Random random)
    {
        return values
            .OrderBy(_ => random.Next())
            .ToList();
    }

    private static int RegionIndex(string region, IReadOnlyList<string> regionOrder)
    {
        for (var index = 0; index < regionOrder.Count; index++)
        {
            if (string.Equals(regionOrder[index], region, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return int.MaxValue;
    }

    private static int ColorIndex(string color, IReadOnlyList<string> colorOrder)
    {
        for (var index = 0; index < colorOrder.Count; index++)
        {
            if (string.Equals(colorOrder[index], color, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return int.MaxValue;
    }

    public static string FormatColorLabel(string color)
    {
        return char.ToUpperInvariant(color[0]) + color[1..];
    }
}