using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Italbytz.EA.Graph;
using Italbytz.EA.Individuals;
using Italbytz.EA.Initialization;
using Italbytz.AI.ML.LogicGp.Internal;
using Italbytz.AI.ML.LogicGp.Internal.Trainer.Gecco;
using Italbytz.AI;
using Italbytz.AI.ML.Core;
using Microsoft.ML;

namespace Italbytz.AI.ML.LogicGp.Internal.Trainer.Bioinformatics;

internal class RlcwRunStrategy(
    OperatorGraph algorithmGraph,
    int phase1,
    int phase2Time,
    int folds = 5,
    double minMaxWeight = 0.0)
    : CommonRunStrategy
{
    private int _currentMaxSize;
    private bool sizeDetermination = true;

    public global::Italbytz.AI.Evolutionary.Selection.IValidatedPopulationSelection SelectionStrategy { get; set; } =
        new FinalCandidatesSelection();

    protected override (
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual,
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList)
        RunEvolutionary(IDataView input,
            Dictionary<float, int>[] featureValueMappings,
            Dictionary<uint, int> labelMapping)
    {
        var allIndividuals =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();

        var mlContext = ThreadSafeMLContext.LocalMLContext;

        sizeDetermination = true;
        var chosenSize = phase1 < 0
            ? (-1 * phase1,
                (global::Italbytz.AI.Evolutionary.Individuals.IIndividualList)
                new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation())
            : DetermineModelSize(mlContext, input,
                featureValueMappings, labelMapping);
        allIndividuals.AddRange(chosenSize.Item2);
        _currentMaxSize = chosenSize.Item1 + 1;

        sizeDetermination = false;
        var individualList = TrainAndValidate(input, input,
            featureValueMappings, labelMapping);

        var filteredList = FilterForSize(individualList, chosenSize.Item1);
        if (filteredList.Count > 0)
            individualList = filteredList;

        allIndividuals.AddRange(individualList);

        var bestIndividual = DetermineBestIndividual(allIndividuals,
            g => g.TrainingFitness.ConsolidatedValue);

        return (bestIndividual, allIndividuals);
    }

    private (int,
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList)
        DetermineModelSize(MLContext mlContext,
        IDataView input, Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping)
    {
        var candidates =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        var cvResults = mlContext.Data.CrossValidationSplit(input, folds);


        foreach (var fold in cvResults)
        {
            _currentMaxSize = 15;
            var individuals = TrainAndValidate(fold.TrainSet,
                fold.TestSet, featureValueMappings, labelMapping);
            candidates.AddRange(individuals);
        }

        var paretoFront = ParetoFront(candidates, 15, 0.75);
        var mostCommonSize = GetMostCommonSize(paretoFront);

        var filteredParetoFront = FilterForSize(paretoFront, mostCommonSize);


        //var medianSize = GetMedianSize(paretoFront);

        foreach (var individual in paretoFront)
        {
            if (individual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype genotype)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            Console.WriteLine(
                $"{individual.Size}, {genotype.TrainingFitness.ConsolidatedValue.ToString(CultureInfo.InvariantCulture)}, {genotype.ValidationFitness.ConsolidatedValue.ToString(CultureInfo.InvariantCulture)}");
        }

        return (mostCommonSize, filteredParetoFront);
    }

    private global::Italbytz.AI.Evolutionary.Individuals.IIndividualList
        FilterForSize(
            global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individuals,
            int size)
    {
        var filteredParetoFront =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        foreach (var individual in individuals)
            if (individual.Size == size)
                filteredParetoFront.Add(individual);
        return filteredParetoFront;
    }
    private int GetMedianSize(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList paretoFront)
    {
        var sizes = new List<int>();
        foreach (var individual in paretoFront)
            sizes.Add(individual.Size);
        sizes.Sort();
        var mid = sizes.Count / 2;
        return sizes[mid];
    }

    private global::Italbytz.AI.Evolutionary.Individuals.IIndividualList ParetoFront(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individuals,
        int maxSize, double quantile)
    {
        // Remove duplicate individuals with the same size, training fitness, and validation fitness
        var uniqueIndividuals =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        var seen = new HashSet<string>();

        foreach (var ind in individuals)
        {
            if (ind.Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype validatable)
                continue;

            var key =
                $"{ind.Size}_{validatable.TrainingFitness.ConsolidatedValue}_{validatable.ValidationFitness.ConsolidatedValue}";
            if (!seen.Contains(key))
            {
                uniqueIndividuals.Add(ind);
                seen.Add(key);
            }
        }

        individuals = uniqueIndividuals;

        // Filter individuals by size
        var sizeFiltered =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        foreach (var individual in individuals)
            if (individual.Size <= maxSize)
                sizeFiltered.Add(individual);
        individuals = sizeFiltered;

        // Compute Pareto front
        var paretoFront =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        foreach (var individual in individuals)
        {
            if (individual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            var dominated = false;
            foreach (var otherIndividual in individuals)
            {
                if (otherIndividual == individual) continue;
                if (otherIndividual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype
                    otherValidatable)
                    throw new ArgumentException(
                        "Expected genotype of type IValidatableGenotype");
                if (otherValidatable.TrainingFitness.ConsolidatedValue >=
                    validatable.TrainingFitness.ConsolidatedValue &&
                    otherValidatable.ValidationFitness.ConsolidatedValue >=
                    validatable.ValidationFitness.ConsolidatedValue &&
                    otherIndividual.Size <= individual.Size &&
                    (otherValidatable.TrainingFitness.ConsolidatedValue >
                     validatable.TrainingFitness.ConsolidatedValue ||
                     otherValidatable.ValidationFitness.ConsolidatedValue >
                     validatable.ValidationFitness.ConsolidatedValue
                     || otherIndividual.Size < individual.Size))
                {
                    dominated = true;
                    break;
                }
            }

            if (!dominated) paretoFront.Add(individual);
        }

        // Select top quantile based on validation fitness
        var validationFitnessValues = new double[paretoFront.Count];
        for (var i = 0; i < paretoFront.Count; i++)
        {
            if (paretoFront[i].Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            validationFitnessValues[i] =
                validatable.ValidationFitness.ConsolidatedValue;
        }

        Array.Sort(validationFitnessValues);
        var thresholdIndex =
            (int)(quantile * validationFitnessValues.Length);
        var thresholdValue = validationFitnessValues[
            Math.Clamp(thresholdIndex, 0, validationFitnessValues.Length - 1)];
        var finalParetoFront =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        foreach (var individual in paretoFront)
        {
            if (individual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            if (validatable.ValidationFitness.ConsolidatedValue >=
                thresholdValue)
                finalParetoFront.Add(individual);
        }


        return finalParetoFront;
    }

    private (double, int) MaxValueAndIndex(double[] array)
    {
        var maxValue = double.MinValue;
        var maxIndex = -1;
        for (var i = 0; i < array.Length; i++)
        {
            if (!(array[i] > maxValue)) continue;
            maxValue = array[i];
            maxIndex = i;
        }

        return (maxValue, maxIndex);
    }


    private global::Italbytz.AI.Evolutionary.Individuals.IIndividual DetermineBestIndividual(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individuals,
        Func<global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype, double> metric)
    {
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual? bestIndividual = null;
        var chosenBestFitness = double.MinValue;
        foreach (var individual in individuals)
        {
            if (individual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype
                validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            var validationFitnessValue = metric(validatable);
            if (validationFitnessValue < chosenBestFitness)
                continue;
            chosenBestFitness = validationFitnessValue;
            bestIndividual = individual;
        }

        return bestIndividual;
    }

    private global::Italbytz.AI.Evolutionary.Individuals.IIndividualList BestModelsForGivenSizeAndMetric(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList[] individualLists,
        int targetSize, Func<global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype, double> metric)
    {
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList bestIndividualsList =
            new global::Italbytz.AI.Evolutionary.Individuals.ListBasedPopulation();
        foreach (var list in individualLists)
        {
            var bestFitnessInList = double.MinValue;
            global::Italbytz.AI.Evolutionary.Individuals.IIndividual? bestIndividualInList = null;
            foreach (var individual in list)
                if (individual.Size == targetSize)
                {
                    if (individual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype
                        validatable)
                        throw new ArgumentException(
                            "Expected genotype of type IValidatableGenotype");
                    var fitnessValue =
                        metric(validatable);
                    if (!(fitnessValue > bestFitnessInList)) continue;
                    bestFitnessInList = fitnessValue;
                    bestIndividualInList = individual;
                }

            if (bestIndividualInList != null)
                bestIndividualsList.Add(bestIndividualInList);
        }

        return bestIndividualsList;
    }

    private double CalculateFitnessSumOfBestIndividuals(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individuals,
        Func<global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype, double> metric)
    {
        var sumFitness = 0.0;
        foreach (var individual in individuals)
        {
            if (individual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype
                validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            var validationFitnessValue =
                metric(validatable);
            sumFitness += validationFitnessValue;
        }

        return sumFitness;
    }

    private double CalculateFitnessMedianOfBestIndividuals(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList individuals,
        Func<global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype, double> metric)
    {
        var fitnessValues = new List<double>();
        foreach (var individual in individuals)
        {
            if (individual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype
                validatable)
                throw new ArgumentException(
                    "Expected genotype of type IValidatableGenotype");
            var validationFitnessValue =
                metric(validatable);
            fitnessValues.Add(validationFitnessValue);
        }

        if (fitnessValues.Count == 0) return 0.0;
        fitnessValues.Sort();
        var mid = fitnessValues.Count / 2;
        if (fitnessValues.Count % 2 == 0)
            return (fitnessValues[mid - 1] + fitnessValues[mid]) / 2.0;
        return fitnessValues[mid];
    }

    protected override Task<(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual,
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList)> RunSpecificLogicGp(
        int[][] features, int[] labels)
    {
        return RunLogicGp(features, labels,
            algorithmGraph, new CompleteInitialization(),
            _currentMaxSize,
            generations: int.MaxValue,
            maxTime: sizeDetermination ? phase1 : phase2Time,
            minMaxWeight: minMaxWeight);
    }

    private int GetMostCommonSize(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList population)
    {
        var sizeCounts = new Dictionary<int, int>();

        foreach (var individual in population)
            if (sizeCounts.ContainsKey(individual.Size))
                sizeCounts[individual.Size]++;
            else
                sizeCounts[individual.Size] = 1;

        var mostCommonSize = 0;
        var highestCount = 0;

        foreach (var pair in sizeCounts)
            if (pair.Value > highestCount)
            {
                highestCount = pair.Value;
                mostCommonSize = pair.Key;
            }

        return mostCommonSize;
    }
}