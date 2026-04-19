using System.Collections.Generic;

namespace Italbytz.AI.Probability;

/// <summary>
/// A finite set of possible values for a random variable.
/// </summary>
public interface IFiniteDomain
{
    IReadOnlyList<object> Values { get; }

    int Size { get; }

    bool Contains(object value);
}
