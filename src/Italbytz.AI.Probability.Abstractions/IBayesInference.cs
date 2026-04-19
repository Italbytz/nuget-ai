using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Probability;

/// <summary>
/// An exact or approximate inference algorithm for Bayesian networks.
/// Returns P(queryVars | evidence) as a normalised distribution (AIMA3e ch. 14).
/// </summary>
public interface IBayesInference
{
    IMetrics Metrics { get; }

    ICategoricalDistribution Ask(
        IRandomVariable[] queryVars,
        IAssignmentProposition[] evidence,
        IBayesianNetwork network);
}
