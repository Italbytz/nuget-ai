using System.Collections.Generic;
using Italbytz.AI;

namespace Italbytz.EA.Selection;

internal class UniformSelection : AbstractSelection
{
    public override bool FitnessBasedSelection { get; } = false;

    protected override IEnumerable<global::Italbytz.AI.Evolutionary.Individuals.IIndividual> Select(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individualList,
        int noOfIndividualsToSelect)
    {
        var result =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        for (var i = 0; i < noOfIndividualsToSelect; i++)
            result.Add(individualList[ThreadSafeRandomNetCore.Shared.Next(individualList.Count)]);
        return result;
    }

    public override object Clone()
    {
        return new UniformSelection();
    }
}