using System;
using Italbytz.AI;

namespace Italbytz.EA.Searchspace;

public class TinyGpSearchSpace : global::Italbytz.AI.Evolutionary.SearchSpace.ISearchSpace
{
    public TinyGpSearchSpace()
    {
        Constants = new double[NumberConst];
        for (var i = 0; i < NumberConst; i++)
            Constants[i] =
                ThreadSafeRandomNetCore.Shared.NextDouble() *
                (MaxRandom - MinRandom) + MinRandom;
    }

    public int Depth { get; init; } = 5;

    public int MaxLen { get; init; } = 10000;

    public int MinRandom { get; init; } = -5;
    public int MaxRandom { get; init; } = 5;

    public int VariableCount { get; init; } = 1;
    public int NumberConst { get; init; } = 100;

    public double[] Constants { get; }

    public global::Italbytz.AI.Evolutionary.Individuals.IGenotype
        GetRandomGenotype()
    {
        return TinyGpGenotype.GenerateRandomGenotype(MaxLen, Depth,
            VariableCount, Constants);
    }

    public global::Italbytz.AI.Evolutionary.Individuals.IIndividualList
        GetAStartingPopulation()
    {
        throw new NotImplementedException();
    }
}