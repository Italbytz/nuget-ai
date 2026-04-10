using Italbytz.AI.Evolutionary.Individuals;

namespace Italbytz.AI.Evolutionary.Fitness;

public interface IFitnessFunction
{
    IFitnessValue Evaluate(IIndividual individual);
}
