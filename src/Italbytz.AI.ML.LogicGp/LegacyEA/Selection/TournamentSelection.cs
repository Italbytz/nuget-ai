using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.AI.ML.LogicGp.Internal.Fitness;

namespace Italbytz.EA.Selection;

internal abstract class TournamentSelection : AbstractSelection
{
    public int TournamentSize { get; set; } = 2;

    public override bool FitnessBasedSelection { get; } = true;

    public abstract bool UseDomination { get; }

    protected override IEnumerable<global::Italbytz.AI.Evolutionary.Individuals.IIndividual> Select(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individuals,
        int noOfIndividualsToSelect)
    {
        var individualList =
            individuals as List<global::Italbytz.AI.Evolutionary.Individuals.IIndividual>
            ?? individuals.ToList();
        var selectedIndividuals =
            new global::Italbytz.AI.Evolutionary.Individuals.IIndividual[noOfIndividualsToSelect];
        var rnd = ThreadSafeRandomNetCore.Shared;


        for (var i = 0; i < noOfIndividualsToSelect; i++)
        {
            global::Italbytz.AI.Evolutionary.Individuals.IIndividual? fittest = null;
            global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue? highestFitness = null;
            for (var j = 0; j < TournamentSize; j++)
            {
                var individual =
                    individualList[rnd.Next(individualList.Count)];
                var fitness = individual.LatestKnownFitness;
                if (fittest == null || highestFitness == null ||
                    (UseDomination && fitness.IsDominating(highestFitness)) ||
                    (!UseDomination && fitness.CompareTo(highestFitness) > 0))
                {
                    fittest = individual;
                    highestFitness = fitness;
                }
            }

            selectedIndividuals[i] = fittest;
        }

        return selectedIndividuals;
    }

    public override object Clone()
    {
        throw new NotImplementedException();
    }
}