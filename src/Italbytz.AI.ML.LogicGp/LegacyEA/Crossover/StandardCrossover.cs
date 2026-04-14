using System;
using System.Threading.Tasks;
using Italbytz.EA.Operator;

namespace Italbytz.EA.Crossover;

internal class StandardCrossover : GraphOperator
{
    public override object Clone()
    {
        throw new NotImplementedException();
    }

    public override Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>
        Operate(
        Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> individuals,
        global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction fitnessFunction)
    {
        var individualList = individuals.Result;
        var newPopulation = new ListBasedPopulation();
        for (var i = 0; i < individualList.Count - 1; i += 2)
        {
            var parent = individualList[i];
            var offspring = (global::Italbytz.AI.Evolutionary.Individuals.IIndividual)individualList[i + 1].Clone();
            if (parent.Genotype is not global::Italbytz.AI.Evolutionary.Crossover.ICrossable parentGenotype ||
                offspring.Genotype is not global::Italbytz.AI.Evolutionary.Crossover.ICrossable offspringGenotype)
                throw new InvalidOperationException(
                    "Individual's genotype is not ILogicGpCrossable");
            offspringGenotype.CrossWith(parentGenotype);
            newPopulation.Add(offspring);
        }

        return Task.FromResult<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>(newPopulation);
    }
}