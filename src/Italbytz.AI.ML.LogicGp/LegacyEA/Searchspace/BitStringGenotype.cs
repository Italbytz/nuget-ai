using System;
using System.Collections;
using System.Linq;
using Italbytz.AI.ML.LogicGp.Internal.Fitness;
using Italbytz.EA.Individuals;

namespace Italbytz.EA.Searchspace;

public class BitStringGenotype : IGenotype, global::Italbytz.AI.Evolutionary.Mutation.IMutable
{
    private readonly int _dimension;

    public BitStringGenotype(BitArray bs, int dimension)
    {
        BitArray = bs;
        _dimension = dimension;
    }

    public BitArray BitArray { get; }

    public object Clone()
    {
        return new BitStringGenotype(BitArray, _dimension);
    }

    public global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue?
        LatestKnownFitness { get; set; }
    public int Size { get; }

    public void Mutate(double mutationProbability)
    {
        for (var i = 0; i < BitArray.Count; i++)
            if (Random.Shared.NextDouble() < mutationProbability)
                BitArray[i] = !BitArray[i];
    }

    public override string ToString()
    {
        return BitArray.Cast<bool>()
            .Select(b => b ? "1" : "0")
            .Aggregate(string.Empty, (current, next) => current + next);
    }
}