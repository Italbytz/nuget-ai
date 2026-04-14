using System;
using System.Threading.Tasks;
using Italbytz.EA.Operator;

namespace Italbytz.EA.Mutation;

internal class InsertLiteral : GraphOperator
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
        foreach (var individual in individualList)
        {
            var candidate = (global::Italbytz.AI.Evolutionary.Individuals.IIndividual)individual.Clone();
            if (candidate.Genotype is not global::Italbytz.AI.Evolutionary.Mutation.ILogicGpMutable mutant)
                throw new InvalidOperationException(
                    "Mutant is not ILogicGpMutable");
            mutant.InsertRandomLiteral();
            newPopulation.Add(candidate);
        }

        return Task.FromResult<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>(newPopulation);
    }
}