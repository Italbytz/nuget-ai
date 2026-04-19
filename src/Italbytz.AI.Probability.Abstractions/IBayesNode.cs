using System.Collections.Generic;

namespace Italbytz.AI.Probability;

/// <summary>
/// A node in a Bayesian network, holding the random variable, its conditional
/// probability distribution, and structural links (AIMA3e p. 511).
/// </summary>
public interface IBayesNode
{
    IRandomVariable RandomVariable { get; }

    IReadOnlyList<IBayesNode> Parents { get; }

    IReadOnlyList<IBayesNode> Children { get; }

    /// <summary>
    /// The Markov blanket: parents, children, and parents of children (AIMA3e p. 517).
    /// </summary>
    IReadOnlyList<IBayesNode> MarkovBlanket { get; }

    IConditionalProbabilityDistribution CpD { get; }
}
