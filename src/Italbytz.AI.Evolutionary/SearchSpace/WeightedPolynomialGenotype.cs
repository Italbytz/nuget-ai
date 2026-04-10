using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Evolutionary.Crossover;
using Italbytz.AI.Evolutionary.Fitness;
using Italbytz.AI.Evolutionary.Individuals;

namespace Italbytz.AI.Evolutionary.SearchSpace;

public class WeightedPolynomialGenotype<TLiteral, TCategory>(
    WeightedPolynomial<TLiteral, TCategory> polynomial,
    IList<TLiteral> literals,
    Weighting weighting) : IPredictingGenotype<TCategory>, ICrossable
    where TLiteral : ILiteral<TCategory>
{
    private readonly IList<TLiteral> _literals = literals;

    public WeightedPolynomial<TLiteral, TCategory> Polynomial { get; } = polynomial;

    public Weighting Weighting { get; set; } = weighting;

    public IFitnessValue? LatestKnownFitness { get; set; }

    public int Size => Polynomial.Size;

    public object Clone()
    {
        return new WeightedPolynomialGenotype<TLiteral, TCategory>(
            (WeightedPolynomial<TLiteral, TCategory>)Polynomial.Clone(),
            _literals.ToList(),
            Weighting);
    }

    public void CrossWith(ICrossable parentGenotype)
    {
        if (parentGenotype is not WeightedPolynomialGenotype<TLiteral, TCategory> parent)
        {
            throw new InvalidOperationException("Parent genotype is not of the same type.");
        }

        Polynomial.Monomials.Add((WeightedMonomial<TLiteral, TCategory>)parent.Polynomial.GetRandomMonomial().Clone());
    }

    public float PredictValue(float[] features)
    {
        throw new NotSupportedException("Use class-based prediction for categorical evolutionary genotypes.");
    }

    public float[] PredictValues(float[][] features, float[] labels)
    {
        throw new NotSupportedException("Use class-based prediction for categorical evolutionary genotypes.");
    }

    public int PredictClass(TCategory[] features)
    {
        var result = Polynomial.Evaluate(features);
        if (result.Length == 0)
        {
            return 0;
        }

        var max = result.Max();
        return Array.IndexOf(result, max);
    }

    public int[] PredictClasses(TCategory[][] features, int[] labels)
    {
        var results = new int[features.Length];
        for (var i = 0; i < features.Length; i++)
        {
            results[i] = PredictClass(features[i]);
        }

        return results;
    }

    public override string ToString()
    {
        return Polynomial.ToString();
    }
}
