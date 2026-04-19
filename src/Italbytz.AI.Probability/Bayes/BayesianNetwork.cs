using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Probability.Bayes;

/// <summary>
/// Builds a Bayesian network from an ordered list of nodes and wires up child references.
/// Nodes must be supplied in topological order (parents before children).
/// </summary>
public class BayesianNetwork : IBayesianNetwork
{
    private readonly List<IBayesNode> _nodes;
    private readonly Dictionary<IRandomVariable, IBayesNode> _nodeMap;

    public IReadOnlyList<IBayesNode> Nodes => _nodes;

    public BayesianNetwork(IReadOnlyList<IBayesNode> nodes)
    {
        _nodes = nodes.ToList();
        _nodeMap = nodes.ToDictionary(n => n.RandomVariable);

        // Wire children
        foreach (var node in nodes)
            foreach (var parent in node.Parents)
                if (parent is BayesNode bn && _nodeMap.TryGetValue(parent.RandomVariable, out _))
                    bn.AddChild((BayesNode)node);
    }

    public IBayesNode GetNode(IRandomVariable variable) => _nodeMap[variable];
}
