using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Robotics;

/// <summary>
/// MONTE-CARLO-LOCALIZATION (AIMA3e Fig. 25.9).
/// Particle filter for robot localisation:
///   1. Apply move to each particle (prediction step).
///   2. Weight each particle by P(readings | particle, map) (correction step).
///   3. Resample with replacement according to weights.
/// If all weights collapse below a threshold a new cloud is generated (kidnapped-robot recovery).
/// </summary>
public class MonteCarloLocalization<TPose> : IMonteCarloLocalization<TPose>
    where TPose : IPose<TPose>
{
    private readonly IMap<TPose> _map;
    private readonly Func<IReadOnlyList<TPose>> _cloudGenerator;
    private readonly Random _rng;
    private readonly double _lowWeightThreshold;

    public IMetrics Metrics { get; } = new Metrics();

    /// <param name="map">Occupancy map for ray-casting.</param>
    /// <param name="cloudGenerator">Factory that produces a fresh uniform particle cloud.</param>
    /// <param name="lowWeightThreshold">
    ///   If the normalised weight sum of all surviving particles falls below this fraction,
    ///   inject a fresh cloud (kidnapped-robot recovery).
    /// </param>
    /// <param name="seed">Optional RNG seed for reproducibility.</param>
    public MonteCarloLocalization(
        IMap<TPose> map,
        Func<IReadOnlyList<TPose>> cloudGenerator,
        double lowWeightThreshold = 0.0,
        int? seed = null)
    {
        _map = map;
        _cloudGenerator = cloudGenerator;
        _lowWeightThreshold = lowWeightThreshold;
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public IReadOnlyList<TPose> GenerateCloud(int size) => _cloudGenerator();

    public IReadOnlyList<TPose> Localize(
        IReadOnlyList<TPose> cloud,
        IMove<TPose> move,
        IReadOnlyList<IRangeReading> readings)
    {
        Metrics.Set("particleCount", cloud.Count);

        // Prediction: propagate particles through the motion model
        var predicted = cloud
            .Select(p => move.GenerateNoisySample(p))
            .Where(p => _map.IsPoseValid(p))
            .ToList();

        if (!predicted.Any()) return _cloudGenerator();

        // Correction: compute importance weights
        var weights = predicted
            .Select(p => ComputeWeight(p, readings))
            .ToArray();

        double totalWeight = weights.Sum();

        // Recovery: low effective weight → inject fresh particles
        if (totalWeight < _lowWeightThreshold || totalWeight == 0)
            return _cloudGenerator();

        // Resample
        return WeightedResample(predicted, weights, cloud.Count);
    }

    private double ComputeWeight(TPose pose, IReadOnlyList<IRangeReading> readings)
    {
        double weight = 1.0;
        foreach (var r in readings)
        {
            double expected = _map.RayCast(pose, r.Angle);
            weight *= r.CalculateWeight(expected);
        }
        return weight;
    }

    private List<TPose> WeightedResample(List<TPose> particles, double[] weights, int count)
    {
        double total = weights.Sum();
        var result = new List<TPose>(count);

        // Systematic resampling (lower variance than simple multinomial)
        double step = total / count;
        double position = _rng.NextDouble() * step;
        double cumulative = weights[0];
        int i = 0;

        for (int j = 0; j < count; j++)
        {
            while (position > cumulative && i < particles.Count - 1)
            {
                i++;
                cumulative += weights[i];
            }
            result.Add(particles[i]);
            position += step;
        }

        return result;
    }
}
