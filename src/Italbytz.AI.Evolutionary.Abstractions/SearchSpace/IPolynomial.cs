using System;
using System.Collections.Generic;

namespace Italbytz.AI.Evolutionary.SearchSpace;

public interface IPolynomial<TMonomial, TLiteral, TCategory> : ICloneable
    where TMonomial : IMonomial<TLiteral, TCategory>
    where TLiteral : ILiteral<TCategory>
{
    float[]? Weights { get; set; }

    IList<TMonomial> Monomials { get; set; }

    int Size { get; }

    float[] Evaluate(TCategory[] input);

    TMonomial GetRandomMonomial();

    IList<TLiteral> GetAllLiterals();
}
