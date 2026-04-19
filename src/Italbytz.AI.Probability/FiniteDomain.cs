using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Probability;

/// <summary>A finite domain backed by an explicit list of values.</summary>
public class FiniteDomain : IFiniteDomain
{
    public IReadOnlyList<object> Values { get; }
    public int Size => Values.Count;

    public FiniteDomain(params object[] values)
    {
        Values = Array.AsReadOnly(values);
    }

    public bool Contains(object value) => Values.Contains(value);
}

/// <summary>Boolean domain {true, false}.</summary>
public sealed class BooleanDomain : FiniteDomain
{
    public static readonly BooleanDomain Instance = new();

    public BooleanDomain() : base(true, false) { }
}

/// <summary>Domain of arbitrary string tokens.</summary>
public sealed class ArbitraryTokenDomain : FiniteDomain
{
    public ArbitraryTokenDomain(params string[] tokens)
        : base(tokens.Cast<object>().ToArray()) { }
}
