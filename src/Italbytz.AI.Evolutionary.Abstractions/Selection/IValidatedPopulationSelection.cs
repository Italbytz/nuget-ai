using Italbytz.AI.Evolutionary.Individuals;

namespace Italbytz.AI.Evolutionary.Selection;

public interface IValidatedPopulationSelection
{
    IIndividual Process(IIndividualList[] populations);
}