using System.Collections.Generic;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Robotics;

/// <summary>
/// Monte Carlo Localisation (MCL) (AIMA3e Fig. 25.9).
/// Maintains a particle cloud representing P(pose | history).
/// </summary>
public interface IMonteCarloLocalization<TPose>
{
    IMetrics Metrics { get; }

    /// <summary>
    /// Updates the belief by applying one move and one set of sensor readings.
    /// Returns the updated particle cloud.
    /// </summary>
    IReadOnlyList<TPose> Localize(
        IReadOnlyList<TPose> cloud,
        IMove<TPose> move,
        IReadOnlyList<IRangeReading> readings);

    /// <summary>Generates an initial uniform particle cloud.</summary>
    IReadOnlyList<TPose> GenerateCloud(int size);
}
