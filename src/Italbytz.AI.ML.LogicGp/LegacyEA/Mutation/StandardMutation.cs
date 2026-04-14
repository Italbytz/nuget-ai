using System;
using System.Threading.Tasks;
using Italbytz.EA.Operator;

namespace Italbytz.EA.Mutation;

internal class StandardMutation : GraphOperator
{
    public double MutationProbability { get; set; } = 1.0 / 64.0;

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
            if (candidate.Genotype is not global::Italbytz.AI.Evolutionary.Mutation.IMutable mutant)
                throw new InvalidOperationException("Mutant is not IMutable");
            mutant.Mutate(MutationProbability);
            newPopulation.Add(candidate);
        }

        return Task.FromResult<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>(newPopulation);
    }
}