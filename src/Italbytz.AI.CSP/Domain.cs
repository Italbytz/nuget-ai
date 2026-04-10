using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.CSP;

public class Domain<TVal> : IDomain<TVal>
{
    public Domain(IEnumerable<TVal> values)
    {
        Values = values.ToArray();
    }

    public Domain(params TVal[] values)
    {
        Values = values;
    }

    public TVal[] Values { get; }

    public TVal this[int index] => Values[index];

    public IEnumerator<TVal> GetEnumerator()
    {
        return ((IEnumerable<TVal>)Values).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Equals(IDomain<TVal>? other)
    {
        return other is not null && Values.SequenceEqual(other.Values);
    }

    public bool Contains(TVal value)
    {
        return Values.Contains(value);
    }

    public TVal Get(int index)
    {
        return Values[index];
    }

    public override bool Equals(object? obj)
    {
        return obj is IDomain<TVal> other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var value in Values)
        {
            hash.Add(value);
        }

        return hash.ToHashCode();
    }

    public override string ToString()
    {
        return string.Join(", ", Values);
    }
}
