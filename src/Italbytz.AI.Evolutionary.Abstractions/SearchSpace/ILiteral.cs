using System;

namespace Italbytz.AI.Evolutionary.SearchSpace;

public interface ILiteral<TCategory> : IComparable<ILiteral<TCategory>>
{
    string Label { get; }

    bool Evaluate(TCategory[] input);
}
