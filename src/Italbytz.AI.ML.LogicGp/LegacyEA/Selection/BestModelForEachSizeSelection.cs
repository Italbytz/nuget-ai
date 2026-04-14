using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.ML.LogicGp.Internal.Fitness;

namespace Italbytz.EA.Selection;

internal class BestModelForEachSizeSelection : AbstractSelection
{
    public override bool FitnessBasedSelection { get; } = true;

    protected override IEnumerable<global::Italbytz.AI.Evolutionary.Individuals.IIndividual> Select(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individualList,
        int noOfIndividualsToSelect)
    {
        var population =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();

        var groupedIndividuals =
            individualList.GroupBy(individual => individual.Size);
        foreach (var group in groupedIndividuals)
        {
            var bestIndividual = group.First();
            global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue? bestFitness = null;
            foreach (var individual in group)
            {
                var fitness = individual.LatestKnownFitness;
                if (fitness == null) continue;
                if (bestFitness != null && fitness.CompareTo(bestFitness) <= 0)
                    continue;
                bestFitness = fitness;
                bestIndividual = individual;
            }

/*            Console.WriteLine(((ConfusionAndSizeFitnessValue)bestFitness).Size +
                              "," + bestFitness.ConsolidatedValue.ToString(
                                  CultureInfo.InvariantCulture));*/

            population.Add(bestIndividual);
        }

        return population;
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}