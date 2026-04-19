using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Probability;

/// <summary>
/// A discrete probability distribution over one or more random variables.
/// Internally stored as a flat double[] indexed by a mixed-radix scheme
/// (leftmost variable is most significant).
/// </summary>
public class CategoricalDistribution : ICategoricalDistribution
{
    private readonly IRandomVariable[] _vars;
    private readonly double[] _values;
    private readonly int[] _strides;

    public IReadOnlyList<IRandomVariable> RandomVariables => _vars;

    public CategoricalDistribution(IRandomVariable[] vars, double[] values)
    {
        _vars = vars;
        _values = values;
        _strides = BuildStrides(vars);
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

    private int IndexFor(IAssignmentProposition[] assignments)
    {
        int idx = 0;
        for (int i = 0; i < _vars.Length; i++)
        {
            var ap = Array.Find(assignments, a => a.RandomVariable.Equals(_vars[i]))
                ?? throw new ArgumentException($"No assignment for variable {_vars[i].Name}");
            int valIdx = IndexOfValue(_vars[i].Domain, ap.Value);
            idx += valIdx * _strides[i];
        }
        return idx;
    }

    private static int IndexOfValue(IFiniteDomain domain, object value)
    {
        for (int i = 0; i < domain.Values.Count; i++)
            if (domain.Values[i].Equals(value)) return i;
        throw new ArgumentException($"Value {value} not in domain.");
    }

    public double ValueOf(params IAssignmentProposition[] assignments) =>
        _values[IndexFor(assignments)];

    public ICategoricalDistribution Normalize()
    {
        double sum = _values.Sum();
        return sum == 0
            ? this
            : new CategoricalDistribution(_vars, _values.Select(v => v / sum).ToArray());
    }

    public ICategoricalDistribution Marginalize(IRandomVariable var)
    {
        var remainingVars = _vars.Where(v => !v.Equals(var)).ToArray();
        int newSize = remainingVars.Select(v => v.Domain.Size).Aggregate(1, (a, b) => a * b);
        var newValues = new double[newSize];
        var tempDist = new CategoricalDistribution(remainingVars, newValues);

        ForEach((assignments, prob) =>
        {
            var remainingAssignments = assignments.Where(a => !a.RandomVariable.Equals(var)).ToArray();
            int idx = tempDist.IndexFor(remainingAssignments);
            newValues[idx] += prob;
        });

        return new CategoricalDistribution(remainingVars, newValues);
    }

    public void ForEach(Action<IAssignmentProposition[], double> consumer)
    {
        EnumerateAssignments(_vars, 0, new IAssignmentProposition[_vars.Length], consumer);
    }

    private void EnumerateAssignments(
        IRandomVariable[] vars,
        int dim,
        IAssignmentProposition[] current,
        Action<IAssignmentProposition[], double> consumer)
    {
        if (dim == vars.Length)
        {
            int idx = IndexFor(current);
            consumer(current.ToArray(), _values[idx]);
            return;
        }
        foreach (var val in vars[dim].Domain.Values)
        {
            current[dim] = new AssignmentProposition(vars[dim], val);
            EnumerateAssignments(vars, dim + 1, current, consumer);
        }
    }
}
