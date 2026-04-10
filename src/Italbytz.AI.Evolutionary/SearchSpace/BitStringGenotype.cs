using System;
using System.Collections;
using System.Linq;
using Italbytz.AI.Evolutionary.Fitness;
using Italbytz.AI.Evolutionary.Individuals;
using Italbytz.AI.Evolutionary.Mutation;

namespace Italbytz.AI.Evolutionary.SearchSpace;

public class BitStringGenotype(BitArray bitArray, int dimension) : IGenotype, IMutable
{
    public BitArray BitArray { get; } = new(bitArray);

    public IFitnessValue? LatestKnownFitness { get; set; }

    public int Size { get; } = dimension;

    public object Clone()
    {
        return new BitStringGenotype(new BitArray(BitArray), Size);
    }

    public void Mutate(double mutationProbability)
    {
        for (var i = 0; i < BitArray.Count; i++)
        {
            if (ThreadSafeRandomNetCore.Shared.NextDouble() < mutationProbability)
            {
                BitArray[i] = !BitArray[i];
            }
        }
    }

    public override string ToString()
    {
        return string.Concat(BitArray.Cast<bool>().Select(bit => bit ? '1' : '0'));
    }
}
