using System;
using Italbytz.AI.Evolutionary.Fitness;

namespace Italbytz.AI.Evolutionary.Individuals;

public class Individual(IGenotype genotype, IIndividual[]? parents) : IIndividual
{
    public IIndividual[]? Parents { get; set; } = parents;

    public IGenotype Genotype { get; } = genotype;

    public IFitnessValue? LatestKnownFitness
    {
        get => Genotype.LatestKnownFitness;
        set => Genotype.LatestKnownFitness = value;
    }

    public int Size => Genotype.Size;

    public int Generation { get; set; }

    public bool IsDominating(IIndividual otherIndividual)
    {
        var fitness = LatestKnownFitness;
        var otherFitness = otherIndividual.LatestKnownFitness;
        if (fitness == null || otherFitness == null)
        {
            throw new InvalidOperationException("Fitness not set.");
        }

        return fitness.IsDominating(otherFitness);
    }

    public object Clone()
    {
        return new Individual((IGenotype)Genotype.Clone(), [this])
        {
            Generation = Generation
        };
    }

    public override string ToString()
    {
        return $"{Genotype} Generation {Generation}, Fitness {LatestKnownFitness}";
    }
}
