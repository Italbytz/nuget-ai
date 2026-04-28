using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Probability.HMM;

public class ParticleFilter
{
    private readonly Random _rng;

    public ParticleFilter(int? seed = null)
    {
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public IReadOnlyList<object> CreateInitialParticles(IHiddenMarkovModel hmm, int particleCount)
    {
        var particles = new List<object>(particleCount);
        for (int i = 0; i < particleCount; i++)
        {
            particles.Add(SampleState(hmm.Prior, hmm.StateVariable.Domain.Values));
        }

        return particles;
    }

    public IReadOnlyList<object> Filter(IHiddenMarkovModel hmm, IReadOnlyList<object> particles, object observation)
    {
        var predicted = particles.Select(particle => SampleTransition(hmm, particle)).ToList();
        var weights = predicted.Select(particle => SensorWeight(hmm, particle, observation)).ToArray();

        var weightSum = weights.Sum();
        if (weightSum <= 0)
        {
            return predicted;
        }

        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] /= weightSum;
        }

        var resampled = new List<object>(predicted.Count);
        for (int i = 0; i < predicted.Count; i++)
        {
            var threshold = _rng.NextDouble();
            var cumulative = 0.0;
            for (int j = 0; j < predicted.Count; j++)
            {
                cumulative += weights[j];
                if (threshold <= cumulative)
                {
                    resampled.Add(predicted[j]);
                    break;
                }
            }

            if (resampled.Count < i + 1)
            {
                resampled.Add(predicted[^1]);
            }
        }

        return resampled;
    }

    private object SampleTransition(IHiddenMarkovModel hmm, object currentState)
    {
        var currentIndex = IndexOfState(hmm, currentState);
        var row = new double[hmm.NumStates];
        for (int i = 0; i < hmm.NumStates; i++)
        {
            row[i] = hmm.TransitionMatrix[currentIndex, i];
        }

        return SampleState(row, hmm.StateVariable.Domain.Values);
    }

    private double SensorWeight(IHiddenMarkovModel hmm, object state, object observation)
    {
        var stateIndex = IndexOfState(hmm, state);
        return hmm.GetSensorDistribution(observation)[stateIndex];
    }

    private int IndexOfState(IHiddenMarkovModel hmm, object state)
    {
        for (int i = 0; i < hmm.StateVariable.Domain.Size; i++)
        {
            if (Equals(hmm.StateVariable.Domain.Values[i], state))
            {
                return i;
            }
        }

        throw new ArgumentException($"State '{state}' is not part of the HMM domain.");
    }

    private object SampleState(double[] distribution, IReadOnlyList<object> values)
    {
        var threshold = _rng.NextDouble();
        var cumulative = 0.0;
        for (int i = 0; i < distribution.Length; i++)
        {
            cumulative += distribution[i];
            if (threshold <= cumulative)
            {
                return values[i];
            }
        }

        return values[^1];
    }
}