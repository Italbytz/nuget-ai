namespace Italbytz.AI.Robotics;

/// <summary>A single range sensor reading at a specific angle.</summary>
public interface IRangeReading
{
    /// <summary>Bearing angle in radians.</summary>
    double Angle { get; }
    /// <summary>Measured range.</summary>
    double Range { get; }
    /// <summary>
    /// Importance weight: P(reading | pose, map).
    /// <paramref name="castDistance"/> is the expected range obtained by ray-casting.
    /// </summary>
    double CalculateWeight(double castDistance);
}
