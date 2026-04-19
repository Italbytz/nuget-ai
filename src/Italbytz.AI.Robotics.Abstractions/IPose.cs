namespace Italbytz.AI.Robotics;

/// <summary>
/// A robot pose (position + orientation).
/// Implementations must support applying a noisy motion command.
/// </summary>
public interface IPose<TSelf>
{
    /// <summary>Returns a new pose by applying the move, possibly with noise.</summary>
    TSelf ApplyMove(IMove<TSelf> move);
}
