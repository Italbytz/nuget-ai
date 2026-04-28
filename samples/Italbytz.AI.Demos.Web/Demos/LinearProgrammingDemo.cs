using System.Globalization;
using Italbytz.AI.Search.Continuous;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record LinearProgrammingConstraint(
    string Label,
    double XCoefficient,
    double YCoefficient,
    double RightHandSide);

internal sealed record LinearProgrammingScenario(
    string Key,
    string Name,
    string Summary,
    string Interpretation,
    double[] Objective,
    IReadOnlyList<LinearProgrammingConstraint> Constraints,
    double AxisMax);

internal sealed record PlotPoint(double X, double Y);

internal sealed record PlotSegment(string Label, PlotPoint Start, PlotPoint End);

internal sealed record FeasibleLatticePoint(int X, int Y, bool IsOptimal);

internal sealed record SubproblemBounds(
    double XLower,
    double XUpper,
    double YLower,
    double YUpper);

internal sealed record BranchAndBoundStep(
    int Number,
    int Depth,
    string NodeLabel,
    string BoundsLabel,
    SubproblemBounds Bounds,
    string Summary,
    string Outcome,
    PlotPoint? RelaxedOptimum,
    double? RelaxedObjective,
    string? BranchVariable,
    double? FloorSplit,
    double? CeilSplit,
    bool IsIncumbent,
    bool IsPruned);

internal sealed record LinearProgrammingAnalysis(
    LinearProgrammingScenario Scenario,
    PlotPoint ContinuousOptimum,
    PlotPoint IntegerOptimum,
    double ContinuousObjective,
    double IntegerObjective,
    IReadOnlyList<PlotPoint> FeasiblePolygon,
    IReadOnlyList<PlotSegment> ConstraintSegments,
    IReadOnlyList<FeasibleLatticePoint> FeasibleIntegerPoints,
    IReadOnlyList<BranchAndBoundStep> BranchTrace);

internal static class LinearProgrammingDemoFactory
{
    public static IReadOnlyList<LinearProgrammingScenario> BuildScenarios()
    {
        return
        [
            new LinearProgrammingScenario(
                "FractionalGap",
                "Fractional LP optimum",
                "The LP relaxation lands between lattice points, while the ILP optimum moves to the nearest admissible integer corner.",
                "Useful for showing why branch-and-bound matters: the continuous optimum is better numerically, but not integer-feasible.",
                [5.0, 6.0],
                [
                    new LinearProgrammingConstraint("x + y <= 5", 1.0, 1.0, 5.0),
                    new LinearProgrammingConstraint("4x + 7y <= 28", 4.0, 7.0, 28.0)
                ],
                6.0),
            new LinearProgrammingScenario(
                "SharedOptimum",
                "Shared LP/ILP optimum",
                "The LP relaxation already ends on an integer corner, so LP and ILP return the same visible solution.",
                "Good for contrasting with the first scenario: sometimes integrality constraints add no extra cost because the best corner is already integral.",
                [6.0, 5.0],
                [
                    new LinearProgrammingConstraint("x + y <= 5", 1.0, 1.0, 5.0),
                    new LinearProgrammingConstraint("3x + 2y <= 12", 3.0, 2.0, 12.0)
                ],
                6.0)
        ];
    }

    public static LinearProgrammingAnalysis Analyze(LinearProgrammingScenario scenario)
    {
        var solver = new LPSolver();
        var continuousModel = CreateModel(scenario, integerVariables: false);
        var integerModel = CreateModel(scenario, integerVariables: true);
        var continuousSolution = solver.Solve(continuousModel);
        var integerSolution = solver.Solve(integerModel);
        var continuousPoint = new PlotPoint(continuousSolution.Solution[0], continuousSolution.Solution[1]);
        var integerPoint = new PlotPoint(integerSolution.Solution[0], integerSolution.Solution[1]);

        return new LinearProgrammingAnalysis(
            scenario,
            continuousPoint,
            integerPoint,
            continuousSolution.Objective,
            integerSolution.Objective,
            BuildFeasiblePolygon(scenario),
            BuildConstraintSegments(scenario),
            BuildFeasibleLattice(scenario, integerPoint),
            BuildBranchTrace(scenario));
    }

    public static string FormatPoint(PlotPoint point)
    {
        return $"({point.X.ToString("0.##", CultureInfo.InvariantCulture)}, {point.Y.ToString("0.##", CultureInfo.InvariantCulture)})";
    }

    public static string FormatObjective(double objective)
    {
        return objective.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static LPModel CreateModel(LinearProgrammingScenario scenario, bool integerVariables)
    {
        return new LPModel
        {
            Maximization = true,
            ObjectiveFunction = scenario.Objective.ToArray(),
            Bounds = [(0.0, scenario.AxisMax), (0.0, scenario.AxisMax)],
            IntegerVariables = [integerVariables, integerVariables],
            Constraints = scenario.Constraints
                .Select(constraint => (ILPConstraint)new LPConstraint
                {
                    Coefficients = [constraint.XCoefficient, constraint.YCoefficient],
                    ConstraintType = ConstraintType.LE,
                    RHS = constraint.RightHandSide
                })
                .ToList()
        };
    }

    private static IReadOnlyList<BranchAndBoundStep> BuildBranchTrace(LinearProgrammingScenario scenario)
    {
        var solver = new LPSolver();
        var steps = new List<BranchAndBoundStep>();
        var counter = 1;
        double? incumbentObjective = null;

        Branch(CreateModel(scenario, integerVariables: true), depth: 0, nodeLabel: "Root");
        return steps;

        void Branch(LPModel integerModel, int depth, string nodeLabel)
        {
            var relaxationModel = CloneWithRelaxedIntegrality(integerModel);
            PlotPoint? relaxedPoint = null;
            double? relaxedObjective = null;

            try
            {
                var relaxedSolution = solver.Solve(relaxationModel);
                relaxedPoint = new PlotPoint(relaxedSolution.Solution[0], relaxedSolution.Solution[1]);
                relaxedObjective = relaxedSolution.Objective;

                if (incumbentObjective.HasValue && relaxedObjective <= incumbentObjective + 0.000001)
                {
                    steps.Add(new BranchAndBoundStep(
                        counter++,
                        depth,
                        nodeLabel,
                        FormatBounds(integerModel.Bounds),
                        ToSubproblemBounds(integerModel.Bounds),
                        $"Solve LP relaxation at {nodeLabel}.",
                        $"Prune this node because its LP upper bound {FormatObjective(relaxedObjective.Value)} is not better than the current incumbent {FormatObjective(incumbentObjective.Value)}.",
                        relaxedPoint,
                        relaxedObjective,
                        BranchVariable: null,
                        FloorSplit: null,
                        CeilSplit: null,
                        IsIncumbent: false,
                        IsPruned: true));
                    return;
                }

                var branchIndex = FindFractionalIndex(relaxedSolution.Solution);
                if (branchIndex < 0)
                {
                    incumbentObjective = relaxedObjective;
                    steps.Add(new BranchAndBoundStep(
                        counter++,
                        depth,
                        nodeLabel,
                        FormatBounds(integerModel.Bounds),
                        ToSubproblemBounds(integerModel.Bounds),
                        $"Solve LP relaxation at {nodeLabel}.",
                        $"The relaxed solution is already integral, so it becomes the incumbent at {FormatPoint(relaxedPoint)} with objective {FormatObjective(relaxedObjective.Value)}.",
                        relaxedPoint,
                        relaxedObjective,
                        BranchVariable: null,
                        FloorSplit: null,
                        CeilSplit: null,
                        IsIncumbent: true,
                        IsPruned: false));
                    return;
                }

                var branchVariable = branchIndex == 0 ? "x" : "y";
                var branchValue = relaxedSolution.Solution[branchIndex];
                var floorValue = Math.Floor(branchValue);
                var ceilValue = Math.Ceiling(branchValue);

                steps.Add(new BranchAndBoundStep(
                    counter++,
                    depth,
                    nodeLabel,
                    FormatBounds(integerModel.Bounds),
                    ToSubproblemBounds(integerModel.Bounds),
                    $"Solve LP relaxation at {nodeLabel}.",
                    $"The LP optimum {FormatPoint(relaxedPoint)} is fractional, so branch on {branchVariable} = {branchValue.ToString("0.##", CultureInfo.InvariantCulture)} into {branchVariable} <= {floorValue.ToString("0", CultureInfo.InvariantCulture)} and {branchVariable} >= {ceilValue.ToString("0", CultureInfo.InvariantCulture)}.",
                    relaxedPoint,
                    relaxedObjective,
                    branchVariable,
                    floorValue,
                    ceilValue,
                    IsIncumbent: false,
                    IsPruned: false));

                var leftModel = DeepCopy(integerModel);
                leftModel.Bounds[branchIndex] = (leftModel.Bounds[branchIndex].Lower, Math.Min(leftModel.Bounds[branchIndex].Upper, floorValue));
                if (leftModel.Bounds[branchIndex].Lower <= leftModel.Bounds[branchIndex].Upper + 0.000001)
                {
                    Branch(leftModel, depth + 1, $"{nodeLabel}L");
                }

                var rightModel = DeepCopy(integerModel);
                rightModel.Bounds[branchIndex] = (Math.Max(rightModel.Bounds[branchIndex].Lower, ceilValue), rightModel.Bounds[branchIndex].Upper);
                if (rightModel.Bounds[branchIndex].Lower <= rightModel.Bounds[branchIndex].Upper + 0.000001)
                {
                    Branch(rightModel, depth + 1, $"{nodeLabel}R");
                }
            }
            catch (InvalidOperationException)
            {
                steps.Add(new BranchAndBoundStep(
                    counter++,
                    depth,
                    nodeLabel,
                    FormatBounds(integerModel.Bounds),
                    ToSubproblemBounds(integerModel.Bounds),
                    $"Solve LP relaxation at {nodeLabel}.",
                    "This branch is infeasible under the current variable bounds and is discarded immediately.",
                    relaxedPoint,
                    relaxedObjective,
                    BranchVariable: null,
                    FloorSplit: null,
                    CeilSplit: null,
                    IsIncumbent: false,
                    IsPruned: true));
            }
        }
    }

    private static IReadOnlyList<PlotPoint> BuildFeasiblePolygon(LinearProgrammingScenario scenario)
    {
        var candidates = new List<PlotPoint>
        {
            new(0, 0),
            new(0, scenario.AxisMax),
            new(scenario.AxisMax, 0),
            new(scenario.AxisMax, scenario.AxisMax)
        };

        foreach (var constraint in scenario.Constraints)
        {
            candidates.AddRange(IntersectionsWithBoundingBox(constraint, scenario.AxisMax));
        }

        for (var i = 0; i < scenario.Constraints.Count; i++)
        {
            for (var j = i + 1; j < scenario.Constraints.Count; j++)
            {
                var intersection = Intersect(scenario.Constraints[i], scenario.Constraints[j]);
                if (intersection is not null)
                {
                    candidates.Add(intersection);
                }
            }
        }

        var feasible = candidates
            .Where(point => IsFeasible(point, scenario))
            .Distinct(new PlotPointComparer())
            .ToList();

        var centerX = feasible.Average(point => point.X);
        var centerY = feasible.Average(point => point.Y);

        return feasible
            .OrderBy(point => Math.Atan2(point.Y - centerY, point.X - centerX))
            .ToArray();
    }

    private static IReadOnlyList<PlotSegment> BuildConstraintSegments(LinearProgrammingScenario scenario)
    {
        return scenario.Constraints
            .Select(constraint =>
            {
                var points = IntersectionsWithBoundingBox(constraint, scenario.AxisMax)
                    .Distinct(new PlotPointComparer())
                    .ToArray();
                return new PlotSegment(constraint.Label, points[0], points[1]);
            })
            .ToArray();
    }

    private static IReadOnlyList<FeasibleLatticePoint> BuildFeasibleLattice(LinearProgrammingScenario scenario, PlotPoint integerOptimum)
    {
        var points = new List<FeasibleLatticePoint>();
        for (var x = 0; x <= (int)scenario.AxisMax; x++)
        {
            for (var y = 0; y <= (int)scenario.AxisMax; y++)
            {
                var point = new PlotPoint(x, y);
                if (!IsFeasible(point, scenario))
                {
                    continue;
                }

                points.Add(new FeasibleLatticePoint(
                    x,
                    y,
                    Math.Abs(x - integerOptimum.X) < 0.000001 && Math.Abs(y - integerOptimum.Y) < 0.000001));
            }
        }

        return points;
    }

    private static bool IsFeasible(PlotPoint point, LinearProgrammingScenario scenario)
    {
        if (point.X < -0.000001 || point.Y < -0.000001 || point.X > scenario.AxisMax + 0.000001 || point.Y > scenario.AxisMax + 0.000001)
        {
            return false;
        }

        return scenario.Constraints.All(constraint =>
            constraint.XCoefficient * point.X + constraint.YCoefficient * point.Y <= constraint.RightHandSide + 0.000001);
    }

    private static LPModel CloneWithRelaxedIntegrality(LPModel model)
    {
        return new LPModel
        {
            Maximization = model.Maximization,
            ObjectiveFunction = model.ObjectiveFunction.ToArray(),
            Bounds = model.Bounds.ToArray(),
            IntegerVariables = model.IntegerVariables.Select(_ => false).ToArray(),
            Constraints = model.Constraints
                .Select(constraint => (ILPConstraint)new LPConstraint
                {
                    Coefficients = constraint.Coefficients.ToArray(),
                    ConstraintType = constraint.ConstraintType,
                    RHS = constraint.RHS
                })
                .ToList()
        };
    }

    private static LPModel DeepCopy(LPModel model)
    {
        return new LPModel
        {
            Maximization = model.Maximization,
            ObjectiveFunction = model.ObjectiveFunction.ToArray(),
            Bounds = model.Bounds.ToArray(),
            IntegerVariables = model.IntegerVariables.ToArray(),
            Constraints = model.Constraints
                .Select(constraint => (ILPConstraint)new LPConstraint
                {
                    Coefficients = constraint.Coefficients.ToArray(),
                    ConstraintType = constraint.ConstraintType,
                    RHS = constraint.RHS
                })
                .ToList()
        };
    }

    private static int FindFractionalIndex(IReadOnlyList<double> solution)
    {
        for (var index = 0; index < solution.Count; index++)
        {
            var rounded = Math.Round(solution[index]);
            if (Math.Abs(solution[index] - rounded) > 0.000001)
            {
                return index;
            }
        }

        return -1;
    }

    private static string FormatBounds((double Lower, double Upper)[] bounds)
    {
        return string.Join(", ", bounds.Select((bound, index) =>
            $"{(index == 0 ? "x" : "y")} in [{FormatBound(bound.Lower)}, {FormatBound(bound.Upper)}]"));
    }

    private static SubproblemBounds ToSubproblemBounds((double Lower, double Upper)[] bounds)
    {
        return new SubproblemBounds(bounds[0].Lower, bounds[0].Upper, bounds[1].Lower, bounds[1].Upper);
    }

    private static string FormatBound(double value)
    {
        return double.IsPositiveInfinity(value)
            ? "inf"
            : value.ToString("0.##", CultureInfo.InvariantCulture);
    }

    private static IEnumerable<PlotPoint> IntersectionsWithBoundingBox(LinearProgrammingConstraint constraint, double axisMax)
    {
        var points = new List<PlotPoint>();

        if (Math.Abs(constraint.YCoefficient) > 0.000001)
        {
            points.Add(new PlotPoint(0, constraint.RightHandSide / constraint.YCoefficient));
            points.Add(new PlotPoint(axisMax, (constraint.RightHandSide - constraint.XCoefficient * axisMax) / constraint.YCoefficient));
        }

        if (Math.Abs(constraint.XCoefficient) > 0.000001)
        {
            points.Add(new PlotPoint(constraint.RightHandSide / constraint.XCoefficient, 0));
            points.Add(new PlotPoint((constraint.RightHandSide - constraint.YCoefficient * axisMax) / constraint.XCoefficient, axisMax));
        }

        return points.Where(point => point.X >= -0.000001 && point.Y >= -0.000001 && point.X <= axisMax + 0.000001 && point.Y <= axisMax + 0.000001);
    }

    private static PlotPoint? Intersect(LinearProgrammingConstraint first, LinearProgrammingConstraint second)
    {
        var determinant = first.XCoefficient * second.YCoefficient - second.XCoefficient * first.YCoefficient;
        if (Math.Abs(determinant) < 0.000001)
        {
            return null;
        }

        var x = (first.RightHandSide * second.YCoefficient - second.RightHandSide * first.YCoefficient) / determinant;
        var y = (first.XCoefficient * second.RightHandSide - second.XCoefficient * first.RightHandSide) / determinant;
        return new PlotPoint(x, y);
    }

    private sealed class PlotPointComparer : IEqualityComparer<PlotPoint>
    {
        public bool Equals(PlotPoint? x, PlotPoint? y)
        {
            if (x is null || y is null)
            {
                return x is null && y is null;
            }

            return Math.Abs(x.X - y.X) < 0.000001 && Math.Abs(x.Y - y.Y) < 0.000001;
        }

        public int GetHashCode(PlotPoint obj)
        {
            return HashCode.Combine(Math.Round(obj.X, 4), Math.Round(obj.Y, 4));
        }
    }
}