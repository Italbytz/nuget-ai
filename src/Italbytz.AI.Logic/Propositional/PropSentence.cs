using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Logic.Propositional;

namespace Italbytz.AI.Logic.Propositional;

/// <summary>A propositional symbol (atomic sentence).</summary>
public class PropSymbol : IPropSentence
{
    public string Name { get; }
    public IEnumerable<string> Symbols => new[] { Name };

    public PropSymbol(string name) => Name = name;

    public bool IsTrue(IDictionary<string, bool> model) =>
        model.TryGetValue(Name, out var v) && v;

    public bool IsFalse(IDictionary<string, bool> model) =>
        model.TryGetValue(Name, out var v) && !v;

    public override string ToString() => Name;
}

/// <summary>Connective types for complex sentences.</summary>
public enum Connective { NOT, AND, OR, IMPLIES, BICONDITIONAL }

/// <summary>A complex propositional sentence formed by a connective and one or two sub-sentences.</summary>
public class ConnectedSentence : IPropSentence
{
    public Connective Connective { get; }
    public IPropSentence Left { get; }
    public IPropSentence? Right { get; }

    public IEnumerable<string> Symbols =>
        Right is null ? Left.Symbols : Left.Symbols.Concat(Right.Symbols).Distinct();

    public ConnectedSentence(Connective connective, IPropSentence left, IPropSentence? right = null)
    {
        Connective = connective;
        Left = left;
        Right = right;
    }

    public bool IsTrue(IDictionary<string, bool> model) => Connective switch
    {
        Connective.NOT => Left.IsFalse(model),
        Connective.AND => Left.IsTrue(model) && Right!.IsTrue(model),
        Connective.OR => Left.IsTrue(model) || Right!.IsTrue(model),
        Connective.IMPLIES => Left.IsFalse(model) || Right!.IsTrue(model),
        Connective.BICONDITIONAL => Left.IsTrue(model) == Right!.IsTrue(model),
        _ => false
    };

    public bool IsFalse(IDictionary<string, bool> model) => !IsTrue(model);

    public override string ToString() => Connective switch
    {
        Connective.NOT => $"NOT {Left}",
        _ => $"({Left} {Connective} {Right})"
    };
}
