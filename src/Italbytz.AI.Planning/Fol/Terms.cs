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

public class Function : IFunctionTerm
{
    public Function(string symbolicName, IList<ITerm> args)
    {
        SymbolicName = symbolicName;
        Args = new List<ITerm>(args);
    }

    public string SymbolicName { get; }

    public IList<ITerm> Args { get; }

    public bool Equals(ITerm? other)
    {
        return other is IFunctionTerm function
            && string.Equals(SymbolicName, function.SymbolicName, StringComparison.Ordinal)
            && Args.SequenceEqual(function.Args);
    }

    public override bool Equals(object? obj)
    {
        return obj is ITerm term && Equals(term);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(SymbolicName, StringComparer.Ordinal);
        foreach (var arg in Args)
        {
            hash.Add(arg);
        }
        return hash.ToHashCode();
    }

    public override string ToString()
    {
        return $"{SymbolicName}({string.Join(",", Args)})";
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

public class NotSentence : INotSentence
{
    public NotSentence(ISentence negated)
    {
        Negated = negated;
    }

    public string SymbolicName => "NOT";

    public ISentence Negated { get; }

    public override string ToString() => $"~{Negated}";
}

public class ConnectedSentence : IConnectedSentence
{
    public ConnectedSentence(string connector, ISentence first, ISentence second)
    {
        Connector = connector;
        First = first;
        Second = second;
    }

    public string SymbolicName => Connector;

    public string Connector { get; }

    public ISentence First { get; }

    public ISentence Second { get; }

    public override string ToString() => $"({First} {Connector} {Second})";
}

public class QuantifiedSentence : IQuantifiedSentence
{
    public QuantifiedSentence(string quantifier, IList<IVariable> variables, ISentence sentence)
    {
        Quantifier = quantifier;
        Variables = new List<IVariable>(variables);
        Sentence = sentence;
    }

    public string SymbolicName => Quantifier;

    public string Quantifier { get; }

    public IList<IVariable> Variables { get; }

    public ISentence Sentence { get; }

    public override string ToString() => $"{Quantifier} {string.Join(",", Variables)} {Sentence}";
}

public class TermEquality : ITermEquality
{
    public TermEquality(ITerm left, ITerm right)
    {
        Left = left;
        Right = right;
    }

    public string SymbolicName => "=";

    public ITerm Left { get; }

    public ITerm Right { get; }

    public override string ToString() => $"{Left} = {Right}";
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
