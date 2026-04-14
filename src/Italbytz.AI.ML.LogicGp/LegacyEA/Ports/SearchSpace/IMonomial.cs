using System;
using System.Collections.Generic;

namespace Italbytz.EA.SearchSpace;

/// <summary>
///     Represents a monomial in a Genetic Programming search space.
/// </summary>
/// <typeparam name="TLiteral">
///     The type of literal used.
/// </typeparam>
/// <typeparam name="TCategory">
///     The type of the categorical data.
/// </typeparam>
/// <remarks>
///     A monomial consists of a list of literals and a weight vector, and
///     maintains predictions based on the current literals and weights.
/// </remarks>
public interface IMonomial<TLiteral, TCategory> : ICloneable,
    global::Italbytz.AI.Evolutionary.SearchSpace.IMonomial<TLiteral, TCategory>
    where TLiteral : ILiteral<TCategory>
{
    new IList<TLiteral> Literals { get; set; }

    new float[] Weights { get; set; }

    new int Size { get; }

    public new float[] Evaluate(TCategory[] input);

    public new bool EvaluateLiterals(TCategory[] input);
}