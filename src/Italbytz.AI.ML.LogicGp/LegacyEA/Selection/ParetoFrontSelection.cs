using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.EA.Selection;

internal class ParetoFrontSelection : AbstractSelection
{
    public override bool FitnessBasedSelection { get; } = true;

    protected override IEnumerable<global::Italbytz.AI.Evolutionary.Individuals.IIndividual> Select(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individualList,
        int noOfIndividualsToSelect)
    {
        individualList =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation(individualList);
        var maxGeneration =
            individualList.Max(individual => individual.Generation);
        var i = 0;
        while (i < individualList.Count)
        {
            var individual = individualList[i];
            if (individual.Generation < maxGeneration) break;
            var j = i + 1;
            while (j < individualList.Count)
            {
                var otherIndividual = individualList[j];
                if (individual.IsDominating(otherIndividual))
                {
                    individualList.RemoveAt(j);
                }
                else if (otherIndividual.IsDominating(individual))
                {
                    individualList.RemoveAt(i);
                    i--;
                    break;
                }
                else
                {
                    j++;
                }
            }

            i++;
        }

        var population =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        foreach (var individual in individualList) population.Add(individual);
        return population;
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}