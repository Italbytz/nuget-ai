using System;
using System.Collections.Generic;

namespace Italbytz.AI.Probability;

/// <summary>
/// A discrete probability distribution over a set of random variables.
/// Supports normalisation, marginalisation, and iteration over all assignments (AIMA3e p. 487).
/// </summary>
public interface ICategoricalDistribution
{
    IReadOnlyList<IRandomVariable> RandomVariables { get; }

    double ValueOf(params IAssignmentProposition[] assignments);

    ICategoricalDistribution Normalize();

    ICategoricalDistribution Marginalize(IRandomVariable var);

    void ForEach(Action<IAssignmentProposition[], double> consumer);
}
