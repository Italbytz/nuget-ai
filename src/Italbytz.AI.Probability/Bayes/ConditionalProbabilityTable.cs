using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Probability.Bayes;

/// <summary>
/// A Conditional Probability Table (CPT) for a random variable given a list of parent variables.
/// Values are stored as a flat array: for each parent combination (left-to-right, most-significant
/// first), the distribution over all child values is stored in domain order (AIMA3e p. 512).
/// </summary>
public class ConditionalProbabilityTable : IConditionalProbabilityDistribution
{
    private readonly IRandomVariable _on;
    private readonly IRandomVariable[] _parents;
    private readonly double[] _values;

    public IRandomVariable On => _on;
    public IReadOnlyList<IRandomVariable> Parents => _parents;

    /// <param name="on">The child variable.</param>
    /// <param name="parents">Parent variables in the order they appear in the CPT rows.</param>
    /// <param name="values">
    /// Flat array: for each parent combination (enumerated in mixed-radix order,
    /// leftmost parent most significant), one entry per child domain value.
    /// </param>
    public ConditionalProbabilityTable(
        IRandomVariable on,
        IRandomVariable[] parents,
        double[] values)
    {
        _on = on;
        _parents = parents;
        _values = values;
    }

    public ICategoricalDistribution ConditionalOn(params IAssignmentProposition[] parentAssignments)
    {
        int pIdx = ParentIndex(parentAssignments);
        int childSize = _on.Domain.Size;
        var childValues = new double[childSize];
        Array.Copy(_values, pIdx * childSize, childValues, 0, childSize);
        return new CategoricalDistribution(new[] { _on }, childValues);
    }

    public double ValueFor(IAssignmentProposition child, params IAssignmentProposition[] parents)
    {
        int pIdx = ParentIndex(parents);
        int cIdx = IndexOfValue(_on.Domain, child.Value);
        return _values[pIdx * _on.Domain.Size + cIdx];
    }

    private int ParentIndex(IAssignmentProposition[] parentAssignments)
    {
        int index = 0;
        int stride = 1;
        for (int i = _parents.Length - 1; i >= 0; i--)
        {
            var ap = Array.Find(parentAssignments, a => a.RandomVariable.Equals(_parents[i]));
            int valIdx = ap != null ? IndexOfValue(_parents[i].Domain, ap.Value) : 0;
            index += valIdx * stride;
            stride *= _parents[i].Domain.Size;
        }
        return index;
    }

    private static int IndexOfValue(IFiniteDomain domain, object value)
    {
        for (int i = 0; i < domain.Values.Count; i++)
            if (domain.Values[i].Equals(value)) return i;
        throw new ArgumentException($"Value '{value}' not found in domain.");
    }
}
