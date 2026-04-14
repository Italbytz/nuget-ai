using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Italbytz.AI.ML.LogicGp.Internal.Fitness;
using Italbytz.EA.Individuals;
using Italbytz.EA.Selection;

namespace Italbytz.AI.ML.LogicGp.Internal.Trainer.Gecco;

internal class FinalCandidatesSelection :
    global::Italbytz.AI.Evolutionary.Selection.IValidatedPopulationSelection
{
    public global::Italbytz.AI.Evolutionary.Individuals.IIndividual Process(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList[] populations)
    {
        var allFilteredIndividuals =
            new List<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList>();

        foreach (var population in populations)
        {
            foreach (var individual in population)
            {
                if (individual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype genotype)
                    throw new InvalidOperationException(
                        "Genotype does not implement IValidatableGenotype");
                individual.LatestKnownFitness =
                    (global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue?)genotype.ValidationFitness.Clone();
            }

            var filteringSelection = new BestModelForEachSizeSelection();
            var filteredPopulation =
                filteringSelection.Process(Task.FromResult(population), null);
            allFilteredIndividuals.Add(filteredPopulation.Result);
        }

        var allCandidates = allFilteredIndividuals.SelectMany(i => i).ToList();
        var candidatePopulation =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        foreach (var candidate in allCandidates)
            candidatePopulation.Add(candidate);
        return new FinalModelSelection().Process(candidatePopulation)[0];
    }

    global::Italbytz.AI.Evolutionary.Individuals.IIndividual
        global::Italbytz.AI.Evolutionary.Selection.IValidatedPopulationSelection.Process(
            global::Italbytz.AI.Evolutionary.Individuals.IIndividualList[] populations)
        => Process(populations);

    private global::Italbytz.AI.Evolutionary.Individuals.IIndividual?
        ChooseBestIndividual(
            Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> individuals)
    {
        var population = individuals.Result;
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual? bestIndividual = null;
        global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue? bestFitness = null;
        foreach (var individual in population)
        {
            var fitness = individual.LatestKnownFitness;
            if (fitness == null) continue;
            if (bestFitness != null && fitness.CompareTo(bestFitness) <= 0)
                continue;
            bestFitness = fitness;
            bestIndividual = individual;
        }

        return bestIndividual;
    }
}