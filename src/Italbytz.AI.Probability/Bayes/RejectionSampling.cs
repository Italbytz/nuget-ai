using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Probability.Bayes;

public class RejectionSampling : IBayesInference
{
    private readonly int _sampleCount;
    private readonly PriorSample _priorSample;

    public RejectionSampling(int sampleCount = 1000, int? seed = null)
    {
        _sampleCount = sampleCount;
        _priorSample = new PriorSample(seed);
        Metrics = new Metrics();
    }

    public IMetrics Metrics { get; }

    public ICategoricalDistribution Ask(
        IRandomVariable[] queryVars,
        IAssignmentProposition[] evidence,
        IBayesianNetwork network)
    {
        var query = queryVars[0];
        var counts = new double[query.Domain.Size];
        var accepted = 0;

        for (int i = 0; i < _sampleCount; i++)
        {
            var sample = _priorSample.Sample(network);
            if (!evidence.All(ev => sample.TryGetValue(ev.RandomVariable, out var value) && Equals(value, ev.Value)))
            {
                continue;
            }

            accepted++;
            var sampledValue = sample[query];
            for (int j = 0; j < query.Domain.Size; j++)
            {
                if (Equals(query.Domain.Values[j], sampledValue))
                {
                    counts[j]++;
                    break;
                }
            }
        }

        Metrics.Set("sampleCount", _sampleCount);
        Metrics.Set("acceptedSamples", accepted);

        if (accepted == 0)
        {
            return new CategoricalDistribution(queryVars, counts);
        }

        return new CategoricalDistribution(queryVars, counts.Select(c => c / accepted).ToArray());
    }
}