using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Probability;

/// <summary>Asserts that a specific random variable equals a specific value.</summary>
public class AssignmentProposition : IAssignmentProposition
{
    public IRandomVariable RandomVariable { get; }
    public object Value { get; }

    public IEnumerable<IRandomVariable> Scope => new[] { RandomVariable };

    public AssignmentProposition(IRandomVariable randomVariable, object value)
    {
        RandomVariable = randomVariable;
        Value = value;
    }

    public bool Holds(IDictionary<IRandomVariable, object> world) =>
        world.TryGetValue(RandomVariable, out var v) && v.Equals(Value);

    public override string ToString() => $"{RandomVariable.Name}={Value}";

    public override bool Equals(object? obj) =>
        obj is AssignmentProposition ap &&
        ap.RandomVariable.Equals(RandomVariable) &&
        ap.Value.Equals(Value);

    public override int GetHashCode() => HashCode.Combine(RandomVariable, Value);
}

/// <summary>A conjunction of propositions (all must hold).</summary>
public class ConjunctiveProposition : IProposition
{
    private readonly IReadOnlyList<IProposition> _conjuncts;

    public IEnumerable<IRandomVariable> Scope =>
        _conjuncts.SelectMany(c => c.Scope).Distinct();

    public ConjunctiveProposition(IReadOnlyList<IProposition> conjuncts)
    {
        _conjuncts = conjuncts;
    }

    public bool Holds(IDictionary<IRandomVariable, object> world) =>
        _conjuncts.All(c => c.Holds(world));
}
