using System;
using Italbytz.AI.Evolutionary.Fitness;

namespace Italbytz.AI.Evolutionary.Individuals;

public interface IIndividual : ICloneable
{
    IGenotype Genotype { get; }

    IFitnessValue? LatestKnownFitness { get; set; }

    int Size { get; }

    int Generation { get; set; }

    bool IsDominating(IIndividual otherIndividual);
}
