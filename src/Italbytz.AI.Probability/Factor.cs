using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Probability;

/// <summary>
/// An unnormalised factor over a subset of random variables.
/// Used internally by <see cref="EliminationAsk"/> for variable elimination (AIMA3e p. 524).
/// Stored as a dictionary keyed by canonical assignment strings for clarity.
/// </summary>
public class Factor : IFactor
{
    private readonly IRandomVariable[] _vars;
    private readonly Dictionary<string, double> _table;

    public IReadOnlyList<IRandomVariable> ArgumentVariables => _vars;

    public Factor(IRandomVariable[] vars, Dictionary<string, double> table)
    {
        _vars = vars;
        _table = table;
    }

    /// <summary>Creates a factor from a node's CPT, fixing any evidence variables.</summary>
    public static Factor FromNode(IBayesNode node, IAssignmentProposition[] evidence)
    {
        var fixedVars = evidence.Select(e => e.RandomVariable).ToHashSet();
        var freeParents = node.CpD.Parents
            .Where(p => !fixedVars.Contains(p))
            .ToArray();
        var factorVars = new[] { node.RandomVariable }.Concat(freeParents).ToArray();
        var table = new Dictionary<string, double>();

        EnumerateAll(factorVars, 0, new IAssignmentProposition[factorVars.Length],
            assignments =>
            {
                // Build the full set of assignments for the CPD query
                var childAp = assignments.First(a => a.RandomVariable.Equals(node.RandomVariable));
                var parentAps = node.CpD.Parents
                    .Select(p =>
                    {
                        var fromEvidence = Array.Find(evidence, e => e.RandomVariable.Equals(p));
                        if (fromEvidence != null) return fromEvidence;
                        return assignments.First(a => a.RandomVariable.Equals(p));
                    })
                    .ToArray();

                double prob = node.CpD.ValueFor(childAp, parentAps);
                table[MakeKey(assignments)] = prob;
            });

        return new Factor(factorVars, table);
    }

    public double ValueOf(params IAssignmentProposition[] assignments) =>
        _table.GetValueOrDefault(MakeKey(FilterTo(assignments, _vars)), 0.0);

    public IFactor SumOut(IRandomVariable var)
    {
        var newVars = _vars.Where(v => !v.Equals(var)).ToArray();
        var newTable = new Dictionary<string, double>();

        foreach (var (key, val) in _table)
        {
            var assignments = ParseKey(key);
            var remaining = assignments.Where(a => !a.RandomVariable.Equals(var)).ToArray();
            var newKey = MakeKey(remaining);
            newTable[newKey] = newTable.GetValueOrDefault(newKey, 0.0) + val;
        }

        return new Factor(newVars, newTable);
    }

    public IFactor PointwiseProduct(IFactor other)
    {
        var otherFactor = (Factor)other;
        var allVars = _vars.Union(otherFactor._vars, EqualityComparer<IRandomVariable>.Default).ToArray();
        var newTable = new Dictionary<string, double>();

        EnumerateAll(allVars, 0, new IAssignmentProposition[allVars.Length],
            assignments =>
            {
                double p1 = _table.GetValueOrDefault(MakeKey(FilterTo(assignments, _vars)), 0.0);
                double p2 = otherFactor._table.GetValueOrDefault(
                    MakeKey(FilterTo(assignments, otherFactor._vars)), 0.0);
                double product = p1 * p2;
                if (product > 0)
                    newTable[MakeKey(assignments)] = product;
            });

        return new Factor(allVars, newTable);
    }

    /// <summary>Converts this factor to a normalised categorical distribution.</summary>
    public ICategoricalDistribution ToCategoricalDistribution()
    {
        int size = _vars.Select(v => v.Domain.Size).Aggregate(1, (a, b) => a * b);
        var values = new double[size];
        var strides = BuildStrides(_vars);

        foreach (var (key, val) in _table)
        {
            var assignments = ParseKey(key);
            int idx = assignments.Aggregate(0, (acc, ap) =>
            {
                int varIdx = Array.FindIndex(_vars, v => v.Equals(ap.RandomVariable));
                int valIdx = IndexOfValue(_vars[varIdx].Domain, ap.Value);
                return acc + valIdx * strides[varIdx];
            });
            values[idx] = val;
        }

        return new CategoricalDistribution(_vars, values).Normalize();
    }

    private static int[] BuildStrides(IRandomVariable[] vars)
    {
        var strides = new int[vars.Length];
        int stride = 1;
        for (int i = vars.Length - 1; i >= 0; i--)
        {
            strides[i] = stride;
            stride *= vars[i].Domain.Size;
        }
        return strides;
    }

    private static int IndexOfValue(IFiniteDomain domain, object value)
    {
        for (int i = 0; i < domain.Values.Count; i++)
            if (domain.Values[i].Equals(value)) return i;
        return -1;
    }

    internal static string MakeKey(IAssignmentProposition[] assignments) =>
        string.Join(",", assignments
            .OrderBy(a => a.RandomVariable.Name, StringComparer.Ordinal)
            .Select(a => $"{a.RandomVariable.Name}={a.Value}"));

    private IAssignmentProposition[] ParseKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return Array.Empty<IAssignmentProposition>();
        return key.Split(',').Select(pair =>
        {
            var parts = pair.Split('=', 2);
            var varName = parts[0];
            var valStr = parts[1];
            var rv = Array.Find(_vars, v => v.Name == varName)!;
            var domainVal = rv.Domain.Values.First(dv => dv.ToString() == valStr);
            return (IAssignmentProposition)new AssignmentProposition(rv, domainVal);
        }).ToArray();
    }

    private static IAssignmentProposition[] FilterTo(
        IAssignmentProposition[] assignments,
        IRandomVariable[] vars) =>
        assignments.Where(a => vars.Any(v => v.Equals(a.RandomVariable))).ToArray();

    private static void EnumerateAll(
        IRandomVariable[] vars,
        int dim,
        IAssignmentProposition[] current,
        Action<IAssignmentProposition[]> action)
    {
        if (dim == vars.Length)
        {
            action(current.ToArray());
            return;
        }
        foreach (var val in vars[dim].Domain.Values)
        {
            current[dim] = new AssignmentProposition(vars[dim], val);
            EnumerateAll(vars, dim + 1, current, action);
        }
    }
}
