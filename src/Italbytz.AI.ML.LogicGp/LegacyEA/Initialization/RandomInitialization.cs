using System;
using System.Threading.Tasks;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Initialization;

internal class RandomInitialization
    : global::Italbytz.AI.Evolutionary.Initialization.IInitialization
{
    public int Size { get; set; } = 1;
    public global::Italbytz.AI.Evolutionary.SearchSpace.ISearchSpace SearchSpace { get; set; }

    public Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>? Process(
        Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> individuals,
        global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction fitnessFunction)
    {
        var result = new ListBasedPopulation();
        for (var i = 0; i < Size; i++)
            result
                .Add(new Individual(SearchSpace.GetRandomGenotype(),
                    null));
        return Task.FromResult<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>(result);
    }

    Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>?
        global::Italbytz.AI.Evolutionary.Operator.IOperator.Process(
            Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> individuals,
            global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction fitnessFunction)
    {
        return Process(individuals, fitnessFunction);
    }

    public object Clone()
    {
        throw new NotImplementedException();
    }

}