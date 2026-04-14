using System;
using System.Collections;
using Italbytz.AI;

namespace Italbytz.EA.Searchspace;

public class BitString : global::Italbytz.AI.Evolutionary.SearchSpace.ISearchSpace
{
    public BitString(int dimension = 64)
    {
        Dimension = dimension;
    }

    public int Dimension { get; } = 64;

    public global::Italbytz.AI.Evolutionary.Individuals.IGenotype
        GetRandomGenotype()
    {
        var bs = new BitArray(Dimension);
        var random = ThreadSafeRandomNetCore.Shared;
        for (var i = 0; i < Dimension; i++) bs[i] = random.NextDouble() < 0.5;
        return new BitStringGenotype(bs, Dimension);
    }

    public global::Italbytz.AI.Evolutionary.Individuals.IIndividualList
        GetAStartingPopulation()
    {
        throw new NotImplementedException();
    }
}