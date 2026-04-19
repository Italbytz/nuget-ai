using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Logic.Propositional;

namespace Italbytz.AI.Logic.Propositional;

/// <summary>A propositional literal: a symbol, optionally negated.</summary>
public class PropLiteral : IPropLiteral
{
    public string Symbol { get; }
    public bool IsPositive { get; }

    public IEnumerable<string> Symbols => new[] { Symbol };

    public PropLiteral(string symbol, bool positive = true)
    {
        Symbol = symbol;
        IsPositive = positive;
    }

    public bool IsTrue(IDictionary<string, bool> model) =>
        model.TryGetValue(Symbol, out var v) && v == IsPositive;

    public bool IsFalse(IDictionary<string, bool> model) =>
        model.TryGetValue(Symbol, out var v) && v != IsPositive;

    public PropLiteral Complement => new(Symbol, !IsPositive);

    public override string ToString() => IsPositive ? Symbol : $"NOT {Symbol}";

    public override bool Equals(object? obj) =>
        obj is PropLiteral other && other.Symbol == Symbol && other.IsPositive == IsPositive;

    public override int GetHashCode() => HashCode.Combine(Symbol, IsPositive);
}

/// <summary>A disjunction of literals (a clause in CNF).</summary>
public class PropClause : IPropClause
{
    private readonly List<IPropLiteral> _literals;

    public IReadOnlyList<IPropLiteral> Literals => _literals;

    public bool IsEmpty => _literals.Count == 0;

    public bool IsTautology =>
        _literals.Any(l => _literals.Any(l2 => l.Symbol == l2.Symbol && l.IsPositive != l2.IsPositive));

    public PropClause(IEnumerable<IPropLiteral> literals)
    {
        _literals = literals.ToList();
    }

    public override string ToString() =>
        IsEmpty ? "FALSE" : string.Join(" ∨ ", _literals);
}
