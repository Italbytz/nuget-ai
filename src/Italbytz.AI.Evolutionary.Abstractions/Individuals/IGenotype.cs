using System;
using Italbytz.AI.Evolutionary.Fitness;

namespace Italbytz.AI.Evolutionary.Individuals;

public interface IGenotype : ICloneable
{
    IFitnessValue? LatestKnownFitness { get; set; }

    int Size { get; }
}
