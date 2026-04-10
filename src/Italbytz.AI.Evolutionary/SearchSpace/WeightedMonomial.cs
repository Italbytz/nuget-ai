using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Italbytz.AI.Evolutionary.SearchSpace;

public class WeightedMonomial<TLiteral, TCategory>(IList<TLiteral> literals) : IMonomial<TLiteral, TCategory>
    where TLiteral : ILiteral<TCategory>
{
    public IList<TLiteral> Literals { get; set; } = literals;

    public float[]? Weights { get; set; }

    public int Size => Literals.Count;

    public object Clone()
    {
        float[]? weightsCopy = null;
        if (Weights != null)
        {
            weightsCopy = new float[Weights.Length];
            Array.Copy(Weights, weightsCopy, Weights.Length);
        }

        return new WeightedMonomial<TLiteral, TCategory>(Literals.ToList())
        {
            Weights = weightsCopy
        };
    }

    public float[] Evaluate(TCategory[] input)
    {
        var weights = Weights ?? [1f];
        return EvaluateLiterals(input) ? weights : new float[weights.Length];
    }

    public bool EvaluateLiterals(TCategory[] input)
    {
        return Literals.All(literal => literal.Evaluate(input));
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        if (Weights != null)
        {
            builder.Append("  ");
            builder.Append(string.Join(" |  ", Weights.Select(weight => weight.ToString("F2", CultureInfo.InvariantCulture))));
        }

        builder.Append(" | ");
        builder.Append(string.Join(string.Empty, Literals));
        builder.Append(" |");
        return builder.ToString();
    }
}
