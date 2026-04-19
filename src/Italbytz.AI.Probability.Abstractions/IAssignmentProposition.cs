namespace Italbytz.AI.Probability;

/// <summary>
/// Asserts that a random variable has a specific value.
/// The most common form of evidence in Bayesian inference (AIMA3e p. 492).
/// </summary>
public interface IAssignmentProposition : IProposition
{
    IRandomVariable RandomVariable { get; }

    object Value { get; }
}
