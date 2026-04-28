using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Probability.Bayes;

public class PriorSample
{
    private readonly Random _rng;

    public PriorSample(int? seed = null)
    {
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public IDictionary<IRandomVariable, object> Sample(IBayesianNetwork network)
    {
        var sample = new Dictionary<IRandomVariable, object>();
        foreach (var node in network.Nodes)
        {
            var parentAssignments = node.CpD.Parents
                .Select(parent => new AssignmentProposition(parent, sample[parent]))
                .Cast<IAssignmentProposition>()
                .ToArray();

            var distribution = node.CpD.ConditionalOn(parentAssignments);
            sample[node.RandomVariable] = SampleFrom(node.RandomVariable, distribution);
        }

        return sample;
    }

    private object SampleFrom(IRandomVariable variable, ICategoricalDistribution distribution)
    {
        var target = _rng.NextDouble();
        var cumulative = 0.0;
        for (int i = 0; i < variable.Domain.Size; i++)
        {
            var value = variable.Domain.Values[i];
            cumulative += distribution.ValueOf(new AssignmentProposition(variable, value));
            if (target <= cumulative)
            {
                return value;
            }
        }

        return variable.Domain.Values[^1];
    }
}