using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Italbytz.AI.Evolutionary.SearchSpace;

public class WeightedPolynomial<TLiteral, TCategory>(IList<WeightedMonomial<TLiteral, TCategory>> monomials)
    : IPolynomial<WeightedMonomial<TLiteral, TCategory>, TLiteral, TCategory>
    where TLiteral : ILiteral<TCategory>
{
    public IList<WeightedMonomial<TLiteral, TCategory>> Monomials { get; set; } = monomials;

    public float[]? Weights { get; set; }

    public int Size => Monomials.Sum(monomial => monomial.Size);

    public object Clone()
    {
        return new WeightedPolynomial<TLiteral, TCategory>(Monomials.Select(monomial => (WeightedMonomial<TLiteral, TCategory>)monomial.Clone()).ToList())
        {
            Weights = Weights == null ? null : [.. Weights]
        };
    }

    public float[] Evaluate(TCategory[] input)
    {
        if (Monomials.Count == 0)
        {
            return Weights ?? [0f];
        }

        var first = Monomials[0].Evaluate(input);
        var result = new float[first.Length];
        Array.Copy(first, result, first.Length);

        for (var monomialIndex = 1; monomialIndex < Monomials.Count; monomialIndex++)
        {
            var monomialResult = Monomials[monomialIndex].Evaluate(input);
            for (var i = 0; i < result.Length; i++)
            {
                result[i] += monomialResult[i];
            }
        }

        return result.Sum() == 0f && Weights != null ? Weights : result;
    }

    public WeightedMonomial<TLiteral, TCategory> GetRandomMonomial()
    {
        return Monomials[ThreadSafeRandomNetCore.Shared.Next(Monomials.Count)];
    }

    public IList<TLiteral> GetAllLiterals()
    {
        return Monomials.SelectMany(monomial => monomial.Literals).Distinct().ToList();
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append("\n|");
        if (Weights != null)
        {
            for (var i = 0; i < Weights.Length; i++)
            {
                builder.Append($" $w_{i}$ |");
            }

            builder.Append(" Condition                                   |\n|");
            for (var i = 0; i < Weights.Length; i++)
            {
                builder.Append(" ----- |");
            }

            builder.Append(" ------------------------------------------- |\n|  ");
            builder.Append(string.Join(" |  ", Weights.Select(weight => weight.ToString("F2", CultureInfo.InvariantCulture))));
            builder.Append(" | None below fulfilled                        |\n|");
        }

        builder.Append(string.Join("\n|", Monomials));
        builder.Append('\n');
        return builder.ToString();
    }
}
