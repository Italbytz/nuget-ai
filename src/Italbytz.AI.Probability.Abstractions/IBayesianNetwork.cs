using System.Collections.Generic;

namespace Italbytz.AI.Probability;

/// <summary>
/// A Bayesian network: a DAG whose nodes represent random variables and whose
/// structure encodes conditional independence (AIMA3e p. 510).
/// Nodes are stored in topological order.
/// </summary>
public interface IBayesianNetwork
{
    IReadOnlyList<IBayesNode> Nodes { get; }

    IBayesNode GetNode(IRandomVariable variable);
}
