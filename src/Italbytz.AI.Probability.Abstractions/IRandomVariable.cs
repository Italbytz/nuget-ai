namespace Italbytz.AI.Probability;

/// <summary>
/// A named variable that can take values from a finite domain.
/// Corresponds to a random variable in a probability model (AIMA3e p. 486).
/// </summary>
public interface IRandomVariable
{
    string Name { get; }

    IFiniteDomain Domain { get; }
}
