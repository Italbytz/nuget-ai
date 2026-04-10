using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Planning.Fol;

public class Constant : IConstant
{
    public Constant(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public string SymbolicName => Value;

    public bool Equals(IConstant? other)
    {
        return other is not null && string.Equals(Value, other.Value, StringComparison.Ordinal);
    }

    public bool Equals(ITerm? other)
    {
        return other is IConstant constant && Equals(constant);
    }

    public override bool Equals(object? obj)
    {
        return obj is IConstant constant && Equals(constant);
    }

    public override int GetHashCode()
    {
        return StringComparer.Ordinal.GetHashCode(Value);
    }

    public override string ToString()
    {
        return Value;
    }
}

public class Variable : IVariable
{
    public Variable(string symbolicName)
    {
        SymbolicName = symbolicName.Trim();
    }

    public int Indexical { get; } = -1;

    public string SymbolicName { get; }

    public bool Equals(IVariable? other)
    {
        return other is not null
            && string.Equals(SymbolicName, other.SymbolicName, StringComparison.Ordinal)
            && Indexical == other.Indexical;
    }

    public bool Equals(ITerm? other)
    {
        return other is IVariable variable && Equals(variable);
    }

    public override bool Equals(object? obj)
    {
        return obj is IVariable variable && Equals(variable);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StringComparer.Ordinal.GetHashCode(SymbolicName), Indexical);
    }

    public override string ToString()
    {
        return SymbolicName;
    }
}

public class Predicate : IPredicate
{
    public Predicate(string predicateName, IList<ITerm> terms)
    {
        SymbolicName = predicateName;
        Args = new List<ITerm>(terms);
    }

    public string SymbolicName { get; }

    public IList<ITerm> Args { get; }

    public override string ToString()
    {
        return $"{SymbolicName}({string.Join(",", Args)})";
    }
}

public class Literal : ILiteral
{
    private string? _stringRepresentation;

    public Literal(IAtomicSentence atom, bool negated)
    {
        Atom = atom;
        NegativeLiteral = negated;
    }

    public bool NegativeLiteral { get; }

    public bool PositiveLiteral => !NegativeLiteral;

    public IAtomicSentence Atom { get; }

    public ILiteral GetComplementaryLiteral()
    {
        return new Literal(Atom, !NegativeLiteral);
    }

    public bool Equals(ILiteral? other)
    {
        return other is not null
            && NegativeLiteral == other.NegativeLiteral
            && string.Equals(Atom.SymbolicName, other.Atom.SymbolicName, StringComparison.Ordinal)
            && Atom.Args.SequenceEqual(other.Atom.Args);
    }

    public override bool Equals(object? obj)
    {
        return obj is ILiteral literal && Equals(literal);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(NegativeLiteral);
        hash.Add(Atom.SymbolicName, StringComparer.Ordinal);
        foreach (var arg in Atom.Args)
        {
            hash.Add(arg);
        }

        return hash.ToHashCode();
    }

    public override string ToString()
    {
        if (_stringRepresentation is null)
        {
            _stringRepresentation = NegativeLiteral ? $"~{Atom}" : Atom.ToString() ?? string.Empty;
        }

        return _stringRepresentation;
    }
}
