using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI.Probability.HMM;

/// <summary>
/// THE FORWARD-BACKWARD ALGORITHM (AIMA3e Fig. 15.4).
/// Smoothing algorithm that computes the posterior distribution over every hidden
/// state variable given a complete sequence of observations.
/// </summary>
public class ForwardBackwardAlgorithm : IHmmInference
{
    public IMetrics Metrics { get; } = new Metrics();

    public double[] Forward(IHiddenMarkovModel model, double[] prevForward, object observation)
    {
        var sensor = model.GetSensorDistribution(observation);
        int n = model.NumStates;
        var result = new double[n];

        for (int j = 0; j < n; j++)
        {
            double sum = 0;
            for (int i = 0; i < n; i++)
                sum += model.TransitionMatrix[i, j] * prevForward[i];
            result[j] = sensor[j] * sum;
        }

        return Normalize(result);
    }

    public double[] Backward(IHiddenMarkovModel model, double[] nextBackward, object observation)
    {
        var sensor = model.GetSensorDistribution(observation);
        int n = model.NumStates;
        var result = new double[n];

        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                result[i] += model.TransitionMatrix[i, j] * sensor[j] * nextBackward[j];

        return result;
    }

    public IReadOnlyList<double[]> ForwardBackward(
        IHiddenMarkovModel model,
        IReadOnlyList<object> observations,
        double[] prior)
    {
        int T = observations.Count;
        Metrics.Set("sequenceLength", T);

        // Forward pass: fv[k] = P(X_k | e_{1:k})
        var fv = new double[T + 1][];
        fv[0] = prior;
        for (int k = 1; k <= T; k++)
            fv[k] = Forward(model, fv[k - 1], observations[k - 1]);

        // Backward pass + smoothed estimates
        var smoothed = new double[T][];
        var b = new double[model.NumStates];
        for (int i = 0; i < model.NumStates; i++) b[i] = 1.0;

        for (int k = T; k >= 1; k--)
        {
            smoothed[k - 1] = Normalize(fv[k].Zip(b, (f, bv) => f * bv).ToArray());
            b = Backward(model, b, observations[k - 1]);
        }

        return smoothed;
    }

    private static double[] Normalize(double[] v)
    {
        double sum = v.Sum();
        if (sum == 0) return v;
        return v.Select(x => x / sum).ToArray();
    }
}
