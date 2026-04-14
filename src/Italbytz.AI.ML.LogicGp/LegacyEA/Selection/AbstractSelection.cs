using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.EA.Operator;

namespace Italbytz.EA.Selection;

internal abstract class AbstractSelection : GraphOperator
{
    public abstract bool FitnessBasedSelection { get; }
    public override int MaxParents { get; } = int.MaxValue;
    public int NoOfIndividualsToSelect { get; set; } = 1;
    public double RatioOfIndividualsToSelect { get; init; } = 0.5;
    public bool UseRatio { get; init; } = false;

    public override Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>
        Operate(
        Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> individuals,
        global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction fitnessFunction)
    {
        var individualList = individuals.Result;
        var newPopulation =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        if (FitnessBasedSelection)
            foreach (var individual in individualList)
                lock (individual)
                {
                    individual.LatestKnownFitness ??=
                        fitnessFunction.Evaluate(individual);
                }

        var calculatedNoOfIndividualsToSelect = NoOfIndividualsToSelect;
        if (UseRatio)
            calculatedNoOfIndividualsToSelect =
                (int)(RatioOfIndividualsToSelect * individualList.Count);
        var selectedIndividuals =
            Select(individualList, calculatedNoOfIndividualsToSelect);
        foreach (var individual in selectedIndividuals)
            newPopulation.Add(individual);
        return Task.FromResult<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>(newPopulation);
    }

    protected abstract IEnumerable<global::Italbytz.AI.Evolutionary.Individuals.IIndividual> Select(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individualList,
        int noOfIndividualsToSelect);
}