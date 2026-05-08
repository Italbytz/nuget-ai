using System.Collections.Generic;
using System.Globalization;
using Italbytz.AI.Learning.Learners;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record XorPredictionRow(
    string X1,
    string X2,
    string Expected,
    string RawOutput,
    string Predicted,
    bool Correct);

internal sealed record XorLossRow(
    int Epoch,
    string Loss);

internal sealed record XorNeuralNetResult(
    IReadOnlyList<XorPredictionRow> Predictions,
    IReadOnlyList<XorLossRow> LossHistory,
    int Correct,
    int Total,
    string Summary);

internal static class XorNeuralNetDemo
{
    private static readonly double[][] XorInputs =
    [
        [0.0, 0.0],
        [0.0, 1.0],
        [1.0, 0.0],
        [1.0, 1.0]
    ];

    private static readonly double[] XorTargets = [0.0, 1.0, 1.0, 0.0];

    public static XorNeuralNetResult Build(
        int hiddenNeurons = 2,
        double learningRate = 0.5,
        int epochs = 10000,
        int seed = 42)
    {
        var mlp = new MlpLearner(
            layers: [2, hiddenNeurons, 1],
            learningRate: learningRate,
            epochs: epochs,
            seed: seed,
            logEvery: epochs / 10);

        mlp.Train(XorInputs, XorTargets);

        var predictions = new List<XorPredictionRow>();
        int correct = 0;

        for (int i = 0; i < XorInputs.Length; i++)
        {
            double raw = mlp.Predict(XorInputs[i]);
            int predicted = raw >= 0.5 ? 1 : 0;
            int expected = (int)XorTargets[i];
            bool ok = predicted == expected;
            if (ok) correct++;

            predictions.Add(new XorPredictionRow(
                ((int)XorInputs[i][0]).ToString(),
                ((int)XorInputs[i][1]).ToString(),
                expected.ToString(),
                raw.ToString("F4", CultureInfo.InvariantCulture),
                predicted.ToString(),
                ok));
        }

        var lossHistory = new List<XorLossRow>();
        foreach (var (epoch, loss) in mlp.LossHistory)
            lossHistory.Add(new XorLossRow(epoch, loss.ToString("F6", CultureInfo.InvariantCulture)));

        var summary =
            $"MLP 2→{hiddenNeurons}→1, sigmoid, batch backprop. " +
            $"lr={learningRate}, epochs={epochs}, seed={seed}. " +
            $"Accuracy: {correct}/{XorInputs.Length}.";

        return new XorNeuralNetResult(predictions, lossHistory, correct, XorInputs.Length, summary);
    }
}
