using System;

namespace Italbytz.AI.CSP;

public class Variable : IVariable
{
    public Variable(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public bool Equals(IVariable? other)
    {
        return other is not null && string.Equals(Name, other.Name, StringComparison.Ordinal);
    }

    public override bool Equals(object? obj)
    {
        return obj is IVariable other && Equals(other);
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Name);
    }

    public override string ToString()
    {
        return Name;
    }
}
