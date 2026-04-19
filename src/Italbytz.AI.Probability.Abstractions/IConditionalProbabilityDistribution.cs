using System.Collections.Generic;

namespace Italbytz.AI.Probability;

/// <summary>
/// A conditional probability distribution P(X | Parents(X)).
/// Used in Bayesian network nodes (AIMA3e p. 512).
/// </summary>
public interface IConditionalProbabilityDistribution
{
    IRandomVariable On { get; }

    IReadOnlyList<IRandomVariable> Parents { get; }

    ICategoricalDistribution ConditionalOn(params IAssignmentProposition[] parentAssignments);

    double ValueFor(IAssignmentProposition child, params IAssignmentProposition[] parents);
}
