using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Probability.Bayes;

/// <summary>
/// A node in a Bayesian network. Children are registered after construction
/// by the <see cref="BayesianNetwork"/> builder.
/// </summary>
public class BayesNode : IBayesNode
{
    private readonly List<IBayesNode> _children = new();
    private List<IBayesNode>? _markovBlanket;

    public IRandomVariable RandomVariable { get; }
    public IReadOnlyList<IBayesNode> Parents { get; }
    public IReadOnlyList<IBayesNode> Children => _children;
    public IConditionalProbabilityDistribution CpD { get; }

    public IReadOnlyList<IBayesNode> MarkovBlanket =>
        _markovBlanket ??= BuildMarkovBlanket();

    public BayesNode(
        IRandomVariable randomVariable,
        IReadOnlyList<IBayesNode> parents,
        IConditionalProbabilityDistribution cpd)
    {
        RandomVariable = randomVariable;
        Parents = parents;
        CpD = cpd;
    }

    internal void AddChild(BayesNode child) => _children.Add(child);

    private List<IBayesNode> BuildMarkovBlanket()
    {
        var blanket = new HashSet<IBayesNode>(Parents);
        foreach (var child in _children)
        {
            blanket.Add(child);
            foreach (var coParent in child.Parents)
                if (!coParent.Equals(this))
                    blanket.Add(coParent);
        }
        return blanket.ToList();
    }
}
