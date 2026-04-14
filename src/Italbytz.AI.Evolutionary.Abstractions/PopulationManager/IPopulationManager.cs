using Italbytz.AI.Evolutionary.Individuals;
using Italbytz.AI.Evolutionary.Initialization;

namespace Italbytz.AI.Evolutionary.PopulationManager;

public interface IPopulationManager
{
    IIndividualList Population { get; set; }

    void InitPopulation(IInitialization initialization);

    void Freeze();
}