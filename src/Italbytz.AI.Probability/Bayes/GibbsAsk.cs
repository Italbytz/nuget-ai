using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Probability.Bayes;

/// <summary>
/// GIBBS-ASK — Markov Chain Monte Carlo inference (AIMA3e Fig. 14.16).
/// Repeatedly resamples each non-evidence variable from its Markov blanket conditional.
/// Converges to the true posterior as the number of samples grows.
/// </summary>
public class GibbsAsk : IBayesInference
{
    private readonly int _sampleCount;
    private readonly Random _rng;

    public IMetrics Metrics { get; } = new Metrics();

    public GibbsAsk(int sampleCount = 1000, int? seed = null)
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
        var counts = new double[X.Domain.Size];

        // Non-evidence variables
        var evidenceVars = evidence.Select(e => e.RandomVariable).ToHashSet();
        var nonEvidenceNodes = network.Nodes
            .Where(n => !evidenceVars.Contains(n.RandomVariable))
            .ToList();

        // Initialize state randomly, respecting evidence
        var state = new Dictionary<IRandomVariable, object>();
        foreach (var e in evidence) state[e.RandomVariable] = e.Value;
        foreach (var node in nonEvidenceNodes)
            state[node.RandomVariable] = node.RandomVariable.Domain.Values[
                _rng.Next(node.RandomVariable.Domain.Size)];

        // Sampling loop
        for (int i = 0; i < _sampleCount; i++)
        {
            foreach (var node in nonEvidenceNodes)
            {
                var rv = node.RandomVariable;
                state[rv] = SampleFromMarkovBlanket(rv, node, state, network);

                // Record count for query variable
                if (rv.Equals(X))
                    for (int j = 0; j < X.Domain.Size; j++)
                        if (X.Domain.Values[j].Equals(state[X]))
                            counts[j]++;
            }
        }

        double sum = counts.Sum();
        return new CategoricalDistribution(queryVars, counts.Select(c => c / sum).ToArray());
    }

    private object SampleFromMarkovBlanket(
        IRandomVariable rv,
        IBayesNode node,
        Dictionary<IRandomVariable, object> state,
        IBayesianNetwork network)
    {
        // P(rv | mb(rv)) ∝ P(rv | parents(rv)) * ∏_child P(child | parents(child))
        var probs = new double[rv.Domain.Size];

        for (int i = 0; i < rv.Domain.Size; i++)
        {
            state[rv] = rv.Domain.Values[i];
            var ap = new AssignmentProposition(rv, rv.Domain.Values[i]);
            var parentAps = node.CpD.Parents
                .Select(p => new AssignmentProposition(p, state[p]))
                .Cast<IAssignmentProposition>()
                .ToArray();

            double p = node.CpD.ValueFor(ap, parentAps);

            foreach (var child in node.Children)
            {
                var childAp = new AssignmentProposition(child.RandomVariable, state[child.RandomVariable]);
                var childParentAps = child.CpD.Parents
                    .Select(pp => new AssignmentProposition(pp, state[pp]))
                    .Cast<IAssignmentProposition>()
                    .ToArray();
                p *= child.CpD.ValueFor(childAp, childParentAps);
            }

            probs[i] = p;
        }

        // Normalise and sample
        double sum = probs.Sum();
        double r = _rng.NextDouble() * sum;
        double cumulative = 0;
        for (int i = 0; i < rv.Domain.Size; i++)
        {
            cumulative += probs[i];
            if (r < cumulative) return rv.Domain.Values[i];
        }
        return rv.Domain.Values[rv.Domain.Size - 1];
    }
}
