using System;

namespace Italbytz.AI.ML.LogicGp.Internal.Fitness;

internal class ConfusionAndSizeFitnessFunction<TCategory> : global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction
    where TCategory : notnull
{
    private readonly bool _compress;
    private readonly TCategory[][] _features;
    private readonly int[] _labels;

    public ConfusionAndSizeFitnessFunction(TCategory[][] features, int[] labels,
        int numberOfObjectives,
        bool compress = true)
    {
        _features = features;
        _labels = labels;
        _compress = compress;
        NumberOfObjectives = numberOfObjectives;
    }

    public int MaxSize { get; set; } = int.MaxValue;

    public int NumberOfObjectives { get; }

    public global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue Evaluate(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual individual)
    {
        if (individual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals
                .IPredictingGenotype<TCategory> genotype)
            throw new ArgumentException(
                "Expected genotype of type IPredictingGenotype");

        return EvaluateCore(individual.Size,
            genotype.PredictClasses(_features, _labels));
    }

    private ConfusionAndSizeFitnessValue EvaluateCore(int size, int[] predictions)
    {
        var confusionTableCounts =
            new int[NumberOfObjectives, NumberOfObjectives];

        if (size > MaxSize)
        {
            for (var i = 0; i < NumberOfObjectives; i++)
            for (var j = 0; j < NumberOfObjectives; j++)
                if (i == j) confusionTableCounts[i, j] = 0;
                else confusionTableCounts[i, j] = 1;

            return new ConfusionAndSizeFitnessValue(
                new ConfusionMatrix(confusionTableCounts), size);
        }

        var featuresLength = _features.Length;

        for (var i = 0; i < featuresLength; i++)
            confusionTableCounts[_labels[i], predictions[i]]++;

        var confusionMatrix = new ConfusionMatrix(confusionTableCounts);

        return new ConfusionAndSizeFitnessValue(confusionMatrix, size,
            _compress);
    }
}