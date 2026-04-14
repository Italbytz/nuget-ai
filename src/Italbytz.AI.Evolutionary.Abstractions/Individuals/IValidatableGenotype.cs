using Italbytz.AI.Evolutionary.Fitness;

namespace Italbytz.AI.Evolutionary.Individuals;

public interface IValidatableGenotype
{
    IFitnessValue? TrainingFitness { get; set; }

    IFitnessValue? ValidationFitness { get; set; }
}