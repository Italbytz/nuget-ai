using System;

namespace Italbytz.AI.ML.LogicGp.Internal.Fitness;

internal class AbsoluteDeviation : global::Italbytz.AI.Evolutionary.Fitness.IFitnessFunction
{
    private readonly float[][] _features;
    private readonly float[] _labels;

    public AbsoluteDeviation(float[][] features, float[] labels)
    {
        _features = features;
        _labels = labels;
    }

    public int NumberOfObjectives { get; } = 1;

    public global::Italbytz.AI.Evolutionary.Fitness.IFitnessValue Evaluate(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividual individual)
    {
        if (individual.Genotype is not global::Italbytz.AI.Evolutionary.Individuals
                .IPredictingGenotype<int> genotype)
            throw new ArgumentException(
                "Expected genotype of type IPredictingGenotype");

        return EvaluateCore(feature => genotype.PredictValue(feature));
    }

    private SingleFitnessValue EvaluateCore(Func<float[], float> predictValue)
    {
        double totalAbsoluteDeviation = 0;
        var featuresLength = _features.Length;

        for (var i = 0; i < featuresLength; i++)
        {
            var prediction = predictValue(_features[i]);
            totalAbsoluteDeviation += Math.Abs(prediction - _labels[i]);
        }

        // Since lower absolute deviation is better, we return its negative
        return new SingleFitnessValue(-totalAbsoluteDeviation);
    }
}