using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Probability.Bayes;

/// <summary>
/// ENUMERATION-ASK (AIMA3e Fig. 14.9).
/// Exact inference by summing over all possible worlds. O(2^n) in the worst case.
/// </summary>
public class EnumerationAsk : IBayesInference
{
    public IMetrics Metrics { get; } = new Metrics();

    public EnumerationAsk()
    {
        Metrics.Set("queriesPerformed", 0);
    }

    public ICategoricalDistribution Ask(
        IRandomVariable[] queryVars,
        IAssignmentProposition[] evidence,
        IBayesianNetwork network)
    {
        Metrics.Set("queriesPerformed", Metrics.GetInt("queriesPerformed") + 1);

        // Single-query-variable version matching AIMA Fig. 14.9
        var X = queryVars[0];
        var rawDist = new double[X.Domain.Size];
        var vars = network.Nodes.ToList();

        for (int i = 0; i < X.Domain.Size; i++)
        {
            var xi = new AssignmentProposition(X, X.Domain.Values[i]);
            var extEvidence = evidence
                .Concat(new IAssignmentProposition[] { xi })
                .ToArray();
            rawDist[i] = EnumerateAll(vars, extEvidence);
        }

        double sum = rawDist.Sum();
        return new CategoricalDistribution(queryVars, rawDist.Select(d => d / sum).ToArray());
    }

    private static double EnumerateAll(
        List<IBayesNode> vars,
        IAssignmentProposition[] evidence)
    {
        if (vars.Count == 0) return 1.0;

        var y = vars[0];
        var rest = vars.Skip(1).ToList();

        var evidenceForY = Array.Find(evidence, ap => ap.RandomVariable.Equals(y.RandomVariable));
        var parentAps = y.CpD.Parents
            .Select(p => Array.Find(evidence, e => e.RandomVariable.Equals(p))!)
            .Where(ap => ap != null)
            .ToArray();

        if (evidenceForY != null)
        {
            double p = y.CpD.ValueFor(evidenceForY, parentAps);
            return p * EnumerateAll(rest, evidence);
        }
        else
        {
            double sum = 0;
            foreach (var val in y.RandomVariable.Domain.Values)
            {
                var yAp = new AssignmentProposition(y.RandomVariable, val);
                var extEvidence = evidence.Concat(new IAssignmentProposition[] { yAp }).ToArray();
                var extParentAps = y.CpD.Parents
                    .Select(p => Array.Find(extEvidence, e => e.RandomVariable.Equals(p))!)
                    .Where(ap => ap != null)
                    .ToArray();
                double p = y.CpD.ValueFor(yAp, extParentAps);
                sum += p * EnumerateAll(rest, extEvidence);
            }
            return sum;
        }
    }
}
