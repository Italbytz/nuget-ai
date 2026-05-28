using System;
using System.Collections.Generic;

namespace Italbytz.AI.Learning;

public static class ClassificationMetrics
{
    public static double Accuracy(IReadOnlyList<string> prediction, IReadOnlyList<string> truth)
    {
        EnsureSameLength(prediction, truth);

        if (prediction.Count == 0)
            return 0.0;

        var correct = 0;
        for (var i = 0; i < prediction.Count; i++)
        {
            if (string.Equals(prediction[i], truth[i], StringComparison.Ordinal))
                correct++;
        }

        return (double)correct / prediction.Count;
    }

    public static double BinaryF1(IReadOnlyList<string> prediction, IReadOnlyList<string> truth, string positiveLabel)
    {
        EnsureSameLength(prediction, truth);

        var tp = 0;
        var fp = 0;
        var fn = 0;

        for (var i = 0; i < prediction.Count; i++)
        {
            var predictedPositive = string.Equals(prediction[i], positiveLabel, StringComparison.Ordinal);
            var truePositive = string.Equals(truth[i], positiveLabel, StringComparison.Ordinal);

            if (predictedPositive && truePositive)
            {
                tp++;
            }
            else if (predictedPositive)
            {
                fp++;
            }
            else if (truePositive)
            {
                fn++;
            }
        }

        var denominator = (2.0 * tp) + fp + fn;
        return denominator > 0.0 ? (2.0 * tp) / denominator : 0.0;
    }

    public static double MacroF1(IReadOnlyList<string> prediction, IReadOnlyList<string> truth, IReadOnlyList<string> classes)
    {
        EnsureSameLength(prediction, truth);

        if (classes.Count == 0)
            return 0.0;

        var f1Sum = 0.0;
        foreach (var klass in classes)
        {
            var tp = 0;
            var fp = 0;
            var fn = 0;

            for (var i = 0; i < prediction.Count; i++)
            {
                var predictedPositive = string.Equals(prediction[i], klass, StringComparison.Ordinal);
                var truePositive = string.Equals(truth[i], klass, StringComparison.Ordinal);

                if (predictedPositive && truePositive)
                {
                    tp++;
                }
                else if (predictedPositive)
                {
                    fp++;
                }
                else if (truePositive)
                {
                    fn++;
                }
            }

            var denominator = (2.0 * tp) + fp + fn;
            var f1 = denominator > 0.0 ? (2.0 * tp) / denominator : 0.0;
            f1Sum += f1;
        }

        return f1Sum / classes.Count;
    }

    private static void EnsureSameLength(IReadOnlyCollection<string> prediction, IReadOnlyCollection<string> truth)
    {
        if (prediction.Count != truth.Count)
            throw new ArgumentException("Prediction and truth must have the same length.");
    }
}