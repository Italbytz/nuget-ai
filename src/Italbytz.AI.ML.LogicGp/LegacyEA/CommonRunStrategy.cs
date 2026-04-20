using System.Collections.Generic;
using System.Threading.Tasks;
using Italbytz.EA;
using Italbytz.AI.ML.LogicGp.Internal.Fitness;
using Italbytz.EA.Graph;
using Italbytz.EA.Individuals;
using Italbytz.EA.Searchspace;
using Italbytz.EA.StoppingCriterion;
using Italbytz.AI;
using Italbytz.AI.ML.Core;
using Microsoft.ML;

namespace Italbytz.AI.ML.LogicGp.Internal;

internal abstract class CommonRunStrategy : global::Italbytz.AI.ML.Core.Control.IRunStrategy
{
    protected int NumberOfObjectives { get; set; }

    (global::Italbytz.AI.Evolutionary.Individuals.IIndividual,
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList)
        global::Italbytz.AI.ML.Core.Control.IRunStrategy.Run(IDataView input,
            Dictionary<float, int>[] featureValueMappings,
            Dictionary<uint, int> labelMapping)
    {
        return RunEvolutionary(input, featureValueMappings, labelMapping);
    }

    protected abstract (
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual,
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList)
        RunEvolutionary(IDataView input,
            Dictionary<float, int>[] featureValueMappings,
            Dictionary<uint, int> labelMapping);


    protected abstract Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> RunSpecificLogicGp(
        int[][] features,
        int[] labels);

    protected virtual Task<global::Italbytz.AI.Evolutionary.Individuals.IIndividualList> RunLogicGp(int[][] features,
        int[] labels, OperatorGraph algorithmGraph,
        global::Italbytz.AI.Evolutionary.Initialization.IInitialization initialization,
        int maxModelSize = int.MaxValue,
        Weighting weighting = Weighting.Computed,
        int generations = int.MaxValue, int maxTime = 60,
        double minMaxWeight = 0.0)
    {
        var logicGp = new EvolutionaryAlgorithm
        {
            FitnessFunction =
                new ConfusionAndSizeFitnessFunction<int>(features, labels,
                    NumberOfObjectives)
                {
                    MaxSize = maxModelSize
                },
            SearchSpace =
                new LogicGpSearchSpace<int>(features, labels, minMaxWeight)
                {
                    Weighting = weighting
                },
            AlgorithmGraph = algorithmGraph
        };
        logicGp.Initialization = initialization;
        initialization.SearchSpace = logicGp.SearchSpace;

        logicGp.StoppingCriteria =
        [
            new GenerationStoppingCriterion(logicGp)
            {
                Limit = generations
            },
            new TimeStoppingCriterion(maxTime)
        ];
        return logicGp.Run();
    }

    public global::Italbytz.AI.Evolutionary.Individuals.IIndividualList TrainAndValidate(IDataView trainSet,
        IDataView validationSet, Dictionary<float, int>[] featureValueMappings,
        Dictionary<uint, int> labelMapping)
    {
        NumberOfObjectives = labelMapping.Count;
        // Train
        var trainExcerpt = trainSet.GetDataExcerpt();
        var trainFeatures = trainExcerpt.Features;
        var trainLabels = trainExcerpt.Labels;
        var convertedTrainFeatures = MappingHelper.MapFeatures(
            trainFeatures,
            featureValueMappings);
        var convertedTrainLabels = MappingHelper.MapLabels(
            trainLabels,
            labelMapping);
        var individuals = RunSpecificLogicGp(convertedTrainFeatures,
            convertedTrainLabels).Result;
        foreach (var individual in individuals)
        {
            (individual.Genotype as global::Italbytz.AI.Evolutionary.Individuals.IFreezable)?.Freeze();
        }
        // Validate
        var validationExcerpt = validationSet.GetDataExcerpt();
        var validationFeatures = validationExcerpt.Features;
        var validationLabels = validationExcerpt.Labels;
        var convertedValidationFeatures =
            MappingHelper.MapFeatures(validationFeatures,
                featureValueMappings);
        var convertedValidationLabels = MappingHelper.MapLabels(
            validationLabels,
            labelMapping);
        var fitness = new ConfusionAndSizeFitnessFunction<int>(
            convertedValidationFeatures, convertedValidationLabels,
            NumberOfObjectives);
        foreach (var individual in individuals)
        {
            var oldFitness =
                (global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue?)individual.LatestKnownFitness.Clone();
            var newFitness = fitness.Evaluate(individual);
            if (individual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals.IValidatableGenotype genotype)
                continue;
            genotype.TrainingFitness = oldFitness;
            genotype.ValidationFitness =
                (global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue?)newFitness.Clone();
        }

        return individuals;
    }
}