using System;
using System.Collections.Generic;

namespace Italbytz.AI.Probability;

/// <summary>A named random variable with a finite domain of possible values.</summary>
public class RandomVariable : IRandomVariable
{
    public string Name { get; }
    public IFiniteDomain Domain { get; }

    public RandomVariable(string name, IFiniteDomain domain)
    {
        Name = name;
        Domain = domain;
    }

    public override string ToString() => Name;

    public override bool Equals(object? obj) =>
        obj is IRandomVariable rv && string.Equals(rv.Name, Name, StringComparison.Ordinal);

    public override int GetHashCode() => Name.GetHashCode(StringComparison.Ordinal);
}
