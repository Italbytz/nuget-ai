using System.Collections.Generic;

namespace Italbytz.AI.Probability;

/// <summary>
/// A constraint over a set of random variables; holds (is true) in some possible worlds.
/// Corresponds to propositions used as queries or evidence (AIMA3e p. 487).
/// </summary>
public interface IProposition
{
    IEnumerable<IRandomVariable> Scope { get; }

    bool Holds(IDictionary<IRandomVariable, object> world);
}
