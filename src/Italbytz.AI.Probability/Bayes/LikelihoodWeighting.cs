using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Probability.Bayes;

/// <summary>
/// LIKELIHOOD-WEIGHTING approximate inference (AIMA3e Fig. 14.15).
/// Generates N weighted samples; evidence variables are fixed and contribute
/// to the weight, non-evidence variables are sampled from their CPTs.
/// More efficient than rejection sampling for rare evidence.
/// </summary>
public class LikelihoodWeighting : IBayesInference
{
    private readonly int _sampleCount;
    private readonly Random _rng;

    public IMetrics Metrics { get; } = new Metrics();

    public LikelihoodWeighting(int sampleCount = 1000, int? seed = null)
    {
        _sampleCount = sampleCount;
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public ICategoricalDistribution Ask(
        IRandomVariable[] queryVars,
        IAssignmentProposition[] evidence,
        IBayesianNetwork network)
    {
        Metrics.Set("sampleCount", _sampleCount);

        var X = queryVars[0];
        var weights = new double[X.Domain.Size];

        for (int i = 0; i < _sampleCount; i++)
        {
            var (sample, weight) = WeightedSample(network, evidence);
            var xValue = sample[X];
            for (int j = 0; j < X.Domain.Size; j++)
                if (X.Domain.Values[j].Equals(xValue))
                    weights[j] += weight;
        }

        double sum = weights.Sum();
        return new CategoricalDistribution(queryVars, weights.Select(w => w / sum).ToArray());
    }

    private (Dictionary<IRandomVariable, object> sample, double weight)
        WeightedSample(IBayesianNetwork network, IAssignmentProposition[] evidence)
    {
        var sample = new Dictionary<IRandomVariable, object>();
        double weight = 1.0;

        foreach (var node in network.Nodes)
        {
            var rv = node.RandomVariable;
            var parentAps = node.CpD.Parents
                .Select(p => new AssignmentProposition(p, sample[p]))
                .Cast<IAssignmentProposition>()
                .ToArray();

            var evidenceForNode = Array.Find(evidence, e => e.RandomVariable.Equals(rv));
            if (evidenceForNode != null)
            {
                // Fix evidence variable and accumulate weight
                sample[rv] = evidenceForNode.Value;
                weight *= node.CpD.ValueFor(evidenceForNode, parentAps);
            }
            else
            {
                // Sample from P(rv | parents)
                var dist = node.CpD.ConditionalOn(parentAps);
                sample[rv] = SampleFrom(rv, dist);
            }
        }

        return (sample, weight);
    }

    private object SampleFrom(IRandomVariable rv, ICategoricalDistribution dist)
    {
        double r = _rng.NextDouble();
        double cumulative = 0;
        for (int i = 0; i < rv.Domain.Size; i++)
        {
            var ap = new AssignmentProposition(rv, rv.Domain.Values[i]);
            cumulative += dist.ValueOf(ap);
            if (r < cumulative) return rv.Domain.Values[i];
        }
        return rv.Domain.Values[rv.Domain.Size - 1];
    }
}
