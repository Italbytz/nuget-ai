using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.EA.Selection;

internal class CutSelection : AbstractSelection
{
    public override bool FitnessBasedSelection { get; } = true;

    protected override IEnumerable<global::Italbytz.AI.Evolutionary.Individuals.IIndividual> Select(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individualList,
        int noOfIndividualsToSelect)
    {
        return individualList
            .OrderByDescending(i => i.LatestKnownFitness)
            .Take(noOfIndividualsToSelect);
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}