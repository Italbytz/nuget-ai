using System;
using System.Collections.Generic;

namespace Italbytz.AI.CSP;

public interface IDomain<TVal> : IEnumerable<TVal>, IEquatable<IDomain<TVal>>
{
    TVal[] Values { get; }

    IList<TVal> AsList => new List<TVal>(Values);

    int Count => Values.Length;

    TVal this[int index] => Values[index];

    bool IsEmpty => Values.Length == 0;
}
