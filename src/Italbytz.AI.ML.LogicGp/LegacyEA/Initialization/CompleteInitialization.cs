using System;
using System.Threading.Tasks;
using Italbytz.EA.SearchSpace;

namespace Italbytz.EA.Initialization;

/// <inheritdoc cref="global::Italbytz.AI.Evolutionary.Initialization.IInitialization" />
internal class CompleteInitialization
    : global::Italbytz.AI.Evolutionary.Initialization.IInitialization
{
    public Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>? Process(
        Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> individuals,
        global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction fitnessFunction)
    {
        var population = SearchSpace.GetAStartingPopulation();
        return Task.FromResult(population);
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

    public global::Italbytz.AI.Evolutionary.SearchSpace.ISearchSpace SearchSpace { get; set; }
}