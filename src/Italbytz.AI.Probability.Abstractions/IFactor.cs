using System.Collections.Generic;

namespace Italbytz.AI.Probability;

/// <summary>
/// An unnormalised function over a subset of random variables.
/// The fundamental data structure for variable elimination (AIMA3e p. 524).
/// </summary>
public interface IFactor
{
    IReadOnlyList<IRandomVariable> ArgumentVariables { get; }

    double ValueOf(params IAssignmentProposition[] assignments);

    /// <summary>Sums out <paramref name="var"/> by marginalising over all its values.</summary>
    IFactor SumOut(IRandomVariable var);

    /// <summary>Pointwise product of this factor with <paramref name="other"/>.</summary>
    IFactor PointwiseProduct(IFactor other);
}
