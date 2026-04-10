using System.Collections;
using Italbytz.AI.Evolutionary.Individuals;

namespace Italbytz.AI.Evolutionary.SearchSpace;

public class BitString(int dimension = 64) : ISearchSpace
{
    public int Dimension { get; } = dimension;

    public IGenotype GetRandomGenotype()
    {
        var bits = new BitArray(Dimension);
        for (var i = 0; i < Dimension; i++)
        {
            bits[i] = ThreadSafeRandomNetCore.Shared.NextDouble() < 0.5;
        }

        return new BitStringGenotype(bits, Dimension);
    }

    public IIndividualList GetAStartingPopulation()
    {
        return new ListBasedPopulation([new Individual(GetRandomGenotype(), null)]);
    }
}
