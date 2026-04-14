using System;
using System.Collections.Generic;
using Italbytz.AI;

namespace Italbytz.EA.Selection;

internal class DropTournamentWorst : AbstractSelection
{
    public int TournamentSize { get; set; } = 2;

    public override bool FitnessBasedSelection { get; } = true;

    protected override IEnumerable<global::Italbytz.AI.Evolutionary.Individuals.IIndividual> Select(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individualList,
        int noOfIndividualsToSelect)
    {
        var count = individualList.Count;
        if (noOfIndividualsToSelect >= count) return individualList;

        var rnd = ThreadSafeRandomNetCore.Shared;
        var result =
            new List<global::Italbytz.AI.Evolutionary.Individuals.IIndividual>(noOfIndividualsToSelect);
        var selected = new bool[count];
        var noOfIndividualsToDrop = count - noOfIndividualsToSelect;
        var dropped = 0;

        while (dropped < noOfIndividualsToDrop)
        {
            global::Italbytz.AI.Evolutionary.Individuals.IIndividual? unfittest = null;
            var unfittestIndex = -1;

            // Select distinct indices for the tournament
            var tournamentIndices = new HashSet<int>();
            while (tournamentIndices.Count < TournamentSize)
            {
                var candidateIndex = rnd.Next(count);
                if (!selected[candidateIndex] &&
                    !tournamentIndices.Contains(candidateIndex))
                    tournamentIndices.Add(candidateIndex);
            }

            foreach (var selectedIndex in tournamentIndices)
            {
                var individual = individualList[selectedIndex];

                if (unfittest != null &&
                    individual.LatestKnownFitness.CompareTo(unfittest
                        .LatestKnownFitness) >= 0) continue;
                unfittest = individual;
                unfittestIndex = selectedIndex;
            }

            if (unfittestIndex == -1 || selected[unfittestIndex]) continue;
            selected[unfittestIndex] = true;
            dropped++;
        }

        for (var i = 0; i < count; i++)
            if (!selected[i])
                result.Add(individualList[i]);

        return result;
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}