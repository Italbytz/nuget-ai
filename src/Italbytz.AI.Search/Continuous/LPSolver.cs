using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Italbytz.AI.Search.Continuous;

/// <summary>
/// Solves dense linear programs through ALGLIB's managed LP backend.
/// </summary>
public class LPSolver : ILPSolver
{
    private const double IntegerTolerance = 0.000001;

    public ILPSolution Solve(ILPModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        ValidateModel(model);

        if (model.IntegerVariables.Any(isInteger => isInteger))
        {
            return SolveInteger(DeepCopy(model));
        }

        return SolveRelaxation(model);
    }

    public ILPSolution Solve(string model, LPFileFormat format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);

        return format switch
        {
            LPFileFormat.lp_solve => Solve(ParseLpSolveModel(model)),
            LPFileFormat.MPS_FIXED => Solve(ParseMpsFixedModel(model)),
            LPFileFormat.MPS_FREE => throw new NotSupportedException("Only MPS fixed-format parsing is currently supported."),
            LPFileFormat.MPS_IBM => throw new NotSupportedException("Only MPS fixed-format parsing is currently supported."),
            LPFileFormat.MPS_NEGOBJCONST => throw new NotSupportedException("Only MPS fixed-format parsing is currently supported."),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    public ILPSolution SolveFile(string filename, LPFileFormat format)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);

        return Solve(File.ReadAllText(filename), format);
    }

    private static ILPSolution SolveInteger(ILPModel model)
    {
        var bestObjective = model.Maximization ? double.NegativeInfinity : double.PositiveInfinity;
        LPSolution? bestSolution = null;

        Branch(model);

        return bestSolution ?? throw new InvalidOperationException("No feasible integer solution exists for the given model.");

        void Branch(ILPModel currentModel)
        {
            if (!TrySolveRelaxation(currentModel, out var relaxedSolution))
            {
                return;
            }

            if (bestSolution is not null)
            {
                if (model.Maximization && relaxedSolution.Objective <= bestObjective + IntegerTolerance)
                {
                    return;
                }

                if (!model.Maximization && relaxedSolution.Objective >= bestObjective - IntegerTolerance)
                {
                    return;
                }
            }

            var branchIndex = FindFractionalIntegerIndex(currentModel.IntegerVariables, relaxedSolution.Solution);
            if (branchIndex < 0)
            {
                bestSolution = relaxedSolution;
                bestObjective = relaxedSolution.Objective;
                return;
            }

            var branchValue = relaxedSolution.Solution[branchIndex];
            var floorValue = Math.Floor(branchValue);
            var ceilValue = Math.Ceiling(branchValue);

            if (floorValue >= currentModel.Bounds[branchIndex].Lower - IntegerTolerance)
            {
                var leftModel = DeepCopy(currentModel);
                leftModel.Bounds[branchIndex] = (leftModel.Bounds[branchIndex].Lower, Math.Min(leftModel.Bounds[branchIndex].Upper, floorValue));
                if (leftModel.Bounds[branchIndex].Lower <= leftModel.Bounds[branchIndex].Upper + IntegerTolerance)
                {
                    Branch(leftModel);
                }
            }

            if (ceilValue <= currentModel.Bounds[branchIndex].Upper + IntegerTolerance)
            {
                var rightModel = DeepCopy(currentModel);
                rightModel.Bounds[branchIndex] = (Math.Max(rightModel.Bounds[branchIndex].Lower, ceilValue), rightModel.Bounds[branchIndex].Upper);
                if (rightModel.Bounds[branchIndex].Lower <= rightModel.Bounds[branchIndex].Upper + IntegerTolerance)
                {
                    Branch(rightModel);
                }
            }
        }
    }

    private static int FindFractionalIntegerIndex(bool[] integerVariables, double[] solution)
    {
        for (var index = 0; index < integerVariables.Length; index++)
        {
            if (!integerVariables[index])
            {
                continue;
            }

            var rounded = Math.Round(solution[index]);
            if (Math.Abs(solution[index] - rounded) > IntegerTolerance)
            {
                return index;
            }
        }

        return -1;
    }

    private static ILPModel DeepCopy(ILPModel model)
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

    private static ILPSolution SolveRelaxation(ILPModel model)
    {
        if (!TrySolveRelaxation(model, out var solution))
        {
            throw new InvalidOperationException("The LP relaxation is infeasible.");
        }

        return solution;
    }

    private static bool TrySolveRelaxation(ILPModel model, out LPSolution solution)
    {
        solution = new LPSolution();

        var variableCount = model.ObjectiveFunction.Length;
        var cost = model.Maximization
            ? model.ObjectiveFunction.Select(coefficient => -coefficient).ToArray()
            : model.ObjectiveFunction.ToArray();
        var lowerBounds = model.Bounds.Select(bound => bound.Lower).ToArray();
        var upperBounds = model.Bounds.Select(bound => bound.Upper).ToArray();

        alglib.minlpcreate(variableCount, out var state);
        alglib.minlpsetcost(state, cost);
        alglib.minlpsetbc(state, lowerBounds, upperBounds);
        alglib.minlpsetscale(state, Enumerable.Repeat(1.0, variableCount).ToArray());

        if (model.Constraints.Count > 0)
        {
            var matrix = new double[model.Constraints.Count, variableCount];
            var lowerConstraintBounds = new double[model.Constraints.Count];
            var upperConstraintBounds = new double[model.Constraints.Count];

            for (var row = 0; row < model.Constraints.Count; row++)
            {
                var constraint = model.Constraints[row];
                for (var column = 0; column < variableCount; column++)
                {
                    matrix[row, column] = constraint.Coefficients[column];
                }

                (lowerConstraintBounds[row], upperConstraintBounds[row]) = ToBounds(constraint);
            }

                alglib.minlpsetlc2dense(state, matrix, lowerConstraintBounds, upperConstraintBounds, model.Constraints.Count);
        }

        alglib.minlpoptimize(state);
        alglib.minlpresults(state, out var rawSolution, out var report);

            if (report.terminationtype <= 0)
            {
                return false;
            }

            solution = new LPSolution
        {
            Objective = Dot(model.ObjectiveFunction, rawSolution),
            Solution = rawSolution
        };

            return true;
    }

        private static LPModel ParseLpSolveModel(string modelText)
    {
            var sanitized = Regex.Replace(modelText, @"//.*?$", string.Empty, RegexOptions.Multiline);
            var statements = sanitized
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(statement => !string.IsNullOrWhiteSpace(statement))
                .ToArray();

            var variableNames = CollectVariableNames(statements);
            var variableIndex = variableNames
                .Select((name, index) => (name, index))
                .ToDictionary(item => item.name, item => item.index, StringComparer.OrdinalIgnoreCase);

            var objective = new double[variableNames.Count];
            var constraints = new List<ILPConstraint>();
            var integerVariables = new bool[variableNames.Count];
            var maximization = false;

            foreach (var statement in statements)
            {
                if (statement.StartsWith("max:", StringComparison.OrdinalIgnoreCase))
                {
                    maximization = true;
                    FillCoefficients(statement[4..], objective, variableIndex);
                    continue;
                }

                if (statement.StartsWith("min:", StringComparison.OrdinalIgnoreCase))
                {
                    maximization = false;
                    FillCoefficients(statement[4..], objective, variableIndex);
                    continue;
                }

                if (statement.StartsWith("int ", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var variable in statement[4..].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        integerVariables[variableIndex[variable]] = true;
                    }

                    continue;
                }

                constraints.Add(ParseLpConstraint(statement, variableIndex, variableNames.Count));
            }

            return new LPModel
            {
                Maximization = maximization,
                ObjectiveFunction = objective,
                Bounds = Enumerable.Repeat((0.0, double.PositiveInfinity), variableNames.Count).ToArray(),
                Constraints = constraints,
                IntegerVariables = integerVariables
            };
        }

        private static LPConstraint ParseLpConstraint(string statement, IReadOnlyDictionary<string, int> variableIndex, int variableCount)
    {
            var operators = new[] { "<=", ">=", "=" };
            foreach (var op in operators)
            {
                var position = statement.IndexOf(op, StringComparison.Ordinal);
                if (position < 0)
                {
                    continue;
                }

                var coefficients = new double[variableCount];
                FillCoefficients(statement[..position], coefficients, variableIndex);
                var rhs = double.Parse(statement[(position + op.Length)..].Trim(), CultureInfo.InvariantCulture);

                return new LPConstraint
                {
                    Coefficients = coefficients,
                    ConstraintType = op switch
                    {
                        "<=" => ConstraintType.LE,
                        ">=" => ConstraintType.GE,
                        _ => ConstraintType.EQ
                    },
                    RHS = rhs
                };
            }

            throw new FormatException($"Unsupported lp_solve statement: {statement}");
    }

        private static void FillCoefficients(string expression, double[] target, IReadOnlyDictionary<string, int> variableIndex)
        {
            foreach (Match match in Regex.Matches(expression, @"([+-]?)\s*(\d+(?:\.\d+)?)\s*\*\s*([A-Za-z]\w*)"))
            {
                var sign = match.Groups[1].Value == "-" ? -1.0 : 1.0;
                var coefficient = sign * double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);
                var variable = match.Groups[3].Value;
                target[variableIndex[variable]] += coefficient;
            }
        }

        private static List<string> CollectVariableNames(IEnumerable<string> statements)
        {
            return Regex.Matches(string.Join("\n", statements), @"[A-Za-z]\w*")
                .Select(match => match.Value)
                .Where(name => !name.Equals("max", StringComparison.OrdinalIgnoreCase)
                    && !name.Equals("min", StringComparison.OrdinalIgnoreCase)
                    && !name.Equals("int", StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => ExtractSortPrefix(name), StringComparer.OrdinalIgnoreCase)
                .ThenBy(name => ExtractSortIndex(name))
                .ThenBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string ExtractSortPrefix(string variableName)
        {
            var match = Regex.Match(variableName, @"^([A-Za-z_]+)");
            return match.Success ? match.Groups[1].Value : variableName;
        }

        private static int ExtractSortIndex(string variableName)
        {
            var match = Regex.Match(variableName, @"(\d+)$");
            return match.Success ? int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) : int.MaxValue;
        }

        private static LPModel ParseMpsFixedModel(string modelText)
        {
            var rowTypes = new Dictionary<string, char>(StringComparer.OrdinalIgnoreCase);
            var rowOrder = new List<string>();
            var constraintRows = new Dictionary<string, Dictionary<string, double>>(StringComparer.OrdinalIgnoreCase);
            var objectiveCoefficients = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var rhsValues = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var lowerBounds = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var upperBounds = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            var integerVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var variableOrder = new List<string>();
            var currentSection = string.Empty;
            var currentIntegerBlock = false;
            string? objectiveRow = null;

            foreach (var rawLine in modelText.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var line = rawLine.TrimEnd('\r');
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var trimmed = line.Trim();
                switch (trimmed)
                {
                    case "NAME":
                    case "ROWS":
                    case "COLUMNS":
                    case "RHS":
                    case "BOUNDS":
                    case "ENDATA":
                        currentSection = trimmed;
                        continue;
                }

                var parts = trimmed.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries);

                switch (currentSection)
                {
                    case "ROWS":
                        rowTypes[parts[1]] = parts[0][0];
                        rowOrder.Add(parts[1]);
                        if (parts[0][0] == 'N')
                        {
                            objectiveRow = parts[1];
                        }
                        break;

                    case "COLUMNS":
                        if (parts.Length >= 3 && parts[1].Equals("'MARKER'", StringComparison.OrdinalIgnoreCase))
                        {
                            currentIntegerBlock = parts[2].Equals("'INTORG'", StringComparison.OrdinalIgnoreCase);
                            if (parts[2].Equals("'INTEND'", StringComparison.OrdinalIgnoreCase))
                            {
                                currentIntegerBlock = false;
                            }
                            break;
                        }

                        var variable = parts[0];
                        if (!variableOrder.Contains(variable, StringComparer.OrdinalIgnoreCase))
                        {
                            variableOrder.Add(variable);
                        }

                        if (currentIntegerBlock)
                        {
                            integerVariables.Add(variable);
                        }

                        AddMpsEntry(parts, 1, variable, objectiveRow, objectiveCoefficients, constraintRows);
                        if (parts.Length >= 5)
                        {
                            AddMpsEntry(parts, 3, variable, objectiveRow, objectiveCoefficients, constraintRows);
                        }
                        break;

                    case "RHS":
                        rhsValues[parts[1]] = double.Parse(parts[2], CultureInfo.InvariantCulture);
                        if (parts.Length >= 5)
                        {
                            rhsValues[parts[3]] = double.Parse(parts[4], CultureInfo.InvariantCulture);
                        }
                        break;

                    case "BOUNDS":
                        ApplyBound(parts, lowerBounds, upperBounds);
                        break;
                }
            }

            var variableIndex = variableOrder
                .Select((name, index) => (name, index))
                .ToDictionary(item => item.name, item => item.index, StringComparer.OrdinalIgnoreCase);
            var objective = new double[variableOrder.Count];
            foreach (var (variable, coefficient) in objectiveCoefficients)
            {
                objective[variableIndex[variable]] = coefficient;
            }

            var constraints = rowOrder
                .Where(row => rowTypes[row] is 'L' or 'G' or 'E')
                .Select(row => new LPConstraint
                {
                    Coefficients = BuildCoefficientArray(variableOrder.Count, variableIndex, constraintRows.TryGetValue(row, out var entries) ? entries : []),
                    ConstraintType = rowTypes[row] switch
                    {
                        'L' => ConstraintType.LE,
                        'G' => ConstraintType.GE,
                        _ => ConstraintType.EQ
                    },
                    RHS = rhsValues.GetValueOrDefault(row, 0.0)
                })
                .Cast<ILPConstraint>()
                .ToList();

            var bounds = variableOrder
                .Select(variable => (
                    lowerBounds.GetValueOrDefault(variable, 0.0),
                    upperBounds.GetValueOrDefault(variable, double.PositiveInfinity)))
                .ToArray();

            var integers = variableOrder.Select(variable => integerVariables.Contains(variable)).ToArray();

            return new LPModel
            {
                Maximization = false,
                ObjectiveFunction = objective,
                Bounds = bounds,
                Constraints = constraints,
                IntegerVariables = integers
            };
        }

        private static void AddMpsEntry(
            string[] parts,
            int index,
            string variable,
            string? objectiveRow,
            IDictionary<string, double> objectiveCoefficients,
            IDictionary<string, Dictionary<string, double>> constraintRows)
        {
            var row = parts[index];
            var value = double.Parse(parts[index + 1], CultureInfo.InvariantCulture);

            if (objectiveRow is not null && row.Equals(objectiveRow, StringComparison.OrdinalIgnoreCase))
            {
                objectiveCoefficients[variable] = value;
                return;
            }

            if (!constraintRows.TryGetValue(row, out var entries))
            {
                entries = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
                constraintRows[row] = entries;
            }

            entries[variable] = value;
        }

        private static void ApplyBound(string[] parts, IDictionary<string, double> lowerBounds, IDictionary<string, double> upperBounds)
        {
            var boundType = parts[0];
            var variable = parts[2];
            var value = parts.Length > 3 ? double.Parse(parts[3], CultureInfo.InvariantCulture) : 0.0;

            switch (boundType)
            {
                case "LO":
                    lowerBounds[variable] = value;
                    break;
                case "UP":
                    upperBounds[variable] = value;
                    break;
                case "FX":
                    lowerBounds[variable] = value;
                    upperBounds[variable] = value;
                    break;
                case "FR":
                    lowerBounds[variable] = double.NegativeInfinity;
                    upperBounds[variable] = double.PositiveInfinity;
                    break;
            }
        }

        private static double[] BuildCoefficientArray(int variableCount, IReadOnlyDictionary<string, int> variableIndex, IReadOnlyDictionary<string, double> coefficients)
        {
            var result = new double[variableCount];
            foreach (var (variable, coefficient) in coefficients)
            {
                result[variableIndex[variable]] = coefficient;
            }

            return result;
        }

    private static (double Lower, double Upper) ToBounds(ILPConstraint constraint)
    {
        return constraint.ConstraintType switch
        {
            ConstraintType.FR => (double.NegativeInfinity, double.PositiveInfinity),
            ConstraintType.LE => (double.NegativeInfinity, constraint.RHS),
            ConstraintType.GE => (constraint.RHS, double.PositiveInfinity),
            ConstraintType.EQ => (constraint.RHS, constraint.RHS),
            ConstraintType.OF => throw new ArgumentException("Objective-function markers are not valid inside the constraint list."),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static double Dot(double[] objective, double[] solution)
    {
        var sum = 0.0;
        for (var index = 0; index < objective.Length; index++)
        {
            sum += objective[index] * solution[index];
        }

        return sum;
    }

    private static void ValidateModel(ILPModel model)
    {
        if (model.ObjectiveFunction.Length == 0)
        {
            throw new ArgumentException("The objective function must contain at least one variable.");
        }

        if (model.Bounds.Length != model.ObjectiveFunction.Length)
        {
            throw new ArgumentException("Bounds must be specified for each objective-function coefficient.");
        }

        if (model.IntegerVariables.Length != 0 && model.IntegerVariables.Length != model.ObjectiveFunction.Length)
        {
            throw new ArgumentException("Integer-variable flags must match the number of objective-function coefficients.");
        }

        foreach (var constraint in model.Constraints)
        {
            if (constraint.Coefficients.Length != model.ObjectiveFunction.Length)
            {
                throw new ArgumentException("Each constraint must provide one coefficient per variable.");
            }
        }
    }
}