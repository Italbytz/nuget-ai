using Italbytz.AI.Evolutionary.Individuals;

namespace Italbytz.AI.Evolutionary.SearchSpace;

public interface ISearchSpace
{
    IGenotype GetRandomGenotype();

    IIndividualList GetAStartingPopulation();
}
