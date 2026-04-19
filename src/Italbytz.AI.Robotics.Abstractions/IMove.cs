namespace Italbytz.AI.Robotics;

/// <summary>A motion command that can be applied to a pose.</summary>
public interface IMove<TPose>
{
    /// <summary>Returns a noisy sample of the move outcome from the given pose.</summary>
    TPose GenerateNoisySample(TPose fromPose);
}
