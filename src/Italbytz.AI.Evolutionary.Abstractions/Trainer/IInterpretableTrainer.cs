using Italbytz.AI.Evolutionary.Individuals;

namespace Italbytz.AI.Evolutionary.Trainer;

public interface IInterpretableTrainer
{
    IIndividualList FinalPopulation { get; }

    IIndividual Model { get; }
}