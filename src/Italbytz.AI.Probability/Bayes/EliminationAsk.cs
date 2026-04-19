using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Probability.Bayes;

/// <summary>
/// ELIMINATION-ASK — Variable Elimination (AIMA3e Fig. 14.11).
/// More efficient than enumeration: factors over hidden variables are summed out
/// one at a time in reverse topological order.
/// </summary>
public class EliminationAsk : IBayesInference
{
    public IMetrics Metrics { get; } = new Metrics();

    public EliminationAsk()
    {
        Metrics.Set("queriesPerformed", 0);
    }

    public ICategoricalDistribution Ask(
        IRandomVariable[] queryVars,
        IAssignmentProposition[] evidence,
        IBayesianNetwork network)
    {
        Metrics.Set("queriesPerformed", Metrics.GetInt("queriesPerformed") + 1);

        var factors = new List<IFactor>();

        // Process variables in reverse topological order
        foreach (var node in network.Nodes.Reverse())
        {
            factors.Add(Factor.FromNode(node, evidence));

            bool isQuery = queryVars.Any(q => q.Equals(node.RandomVariable));
            bool isEvidence = evidence.Any(e => e.RandomVariable.Equals(node.RandomVariable));

            if (!isQuery && !isEvidence)
                factors = SumOut(node.RandomVariable, factors);
        }

        // Multiply remaining factors
        var product = factors.Aggregate((f1, f2) => f1.PointwiseProduct(f2));

        // Normalise and return
        return ((Factor)product).ToCategoricalDistribution();
    }

    private static List<IFactor> SumOut(IRandomVariable var, List<IFactor> factors)
    {
        var relevant = factors.Where(f => f.ArgumentVariables.Contains(var)).ToList();
        var irrelevant = factors.Where(f => !f.ArgumentVariables.Contains(var)).ToList();
        if (relevant.Count == 0) return factors;
        var product = relevant.Aggregate((f1, f2) => f1.PointwiseProduct(f2));
        irrelevant.Add(product.SumOut(var));
        return irrelevant;
    }
}
