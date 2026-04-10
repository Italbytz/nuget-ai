using System;
using System.Collections.Generic;

namespace Italbytz.AI.Evolutionary.SearchSpace;

public interface IMonomial<TLiteral, TCategory> : ICloneable
    where TLiteral : ILiteral<TCategory>
{
    IList<TLiteral> Literals { get; set; }

    float[]? Weights { get; set; }

    int Size { get; }

    float[] Evaluate(TCategory[] input);

    bool EvaluateLiterals(TCategory[] input);
}
