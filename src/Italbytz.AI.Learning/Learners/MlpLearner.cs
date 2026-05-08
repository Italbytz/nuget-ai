using System;
using System.Collections.Generic;

namespace Italbytz.AI.Learning.Learners;

/// <summary>
/// Minimal multi-layer perceptron (MLP) trained with batch backpropagation.
/// Uses sigmoid activation throughout and MSE loss. WASM-compatible (no
/// external dependencies).
/// </summary>
public sealed class MlpLearner
{
    // weights[l][j][k]  — weight from neuron k in layer l-1 to neuron j in layer l
    private readonly double[][][] _w;
    // biases[l][j]
    private readonly double[][] _b;
    private readonly int[] _layers;
    private readonly double _lr;
    private readonly int _epochs;
    private readonly int _logEvery;
    private readonly Random _rng;

    public List<(int Epoch, double Loss)> LossHistory { get; } = new();

    /// <param name="layers">Layer sizes including input and output, e.g. {2,2,1}.</param>
    /// <param name="learningRate">Gradient descent step size.</param>
    /// <param name="epochs">Number of full passes over the training data.</param>
    /// <param name="seed">Random seed for reproducibility.</param>
    /// <param name="logEvery">Record loss every N epochs (0 = only final).</param>
    public MlpLearner(
        int[] layers,
        double learningRate = 0.5,
        int epochs = 10000,
        int seed = 42,
        int logEvery = 1000)
    {
        _layers = layers;
        _lr = learningRate;
        _epochs = epochs;
        _logEvery = logEvery > 0 ? logEvery : epochs;
        _rng = new Random(seed);

        int L = layers.Length;
        _w = new double[L][][];
        _b = new double[L][];

        // Layer 0 is the input — no weights needed.
        _w[0] = Array.Empty<double[]>();
        _b[0] = Array.Empty<double>();

        for (int l = 1; l < L; l++)
        {
            int nIn = layers[l - 1];
            int nOut = layers[l];
            _w[l] = new double[nOut][];
            _b[l] = new double[nOut];
            for (int j = 0; j < nOut; j++)
            {
                _w[l][j] = new double[nIn];
                // Xavier-style initialisation: uniform in [-sqrt(6/(nIn+nOut)), +...]
                double limit = Math.Sqrt(6.0 / (nIn + nOut));
                for (int k = 0; k < nIn; k++)
                    _w[l][j][k] = (_rng.NextDouble() * 2 - 1) * limit;
                _b[l][j] = 0.0;
            }
        }
    }

    // --- Forward pass ---------------------------------------------------------

    /// <summary>Run one forward pass; returns activations for all layers.</summary>
    private double[][] Forward(double[] input)
    {
        int L = _layers.Length;
        var a = new double[L][];
        a[0] = input;
        for (int l = 1; l < L; l++)
        {
            int nOut = _layers[l];
            a[l] = new double[nOut];
            for (int j = 0; j < nOut; j++)
            {
                double z = _b[l][j];
                for (int k = 0; k < _layers[l - 1]; k++)
                    z += _w[l][j][k] * a[l - 1][k];
                a[l][j] = Sigmoid(z);
            }
        }
        return a;
    }

    // --- Training -------------------------------------------------------------

    /// <param name="inputs">Each row is one training example.</param>
    /// <param name="targets">One target value per example (binary 0/1).</param>
    public void Train(double[][] inputs, double[] targets)
    {
        int N = inputs.Length;
        int L = _layers.Length;

        for (int epoch = 1; epoch <= _epochs; epoch++)
        {
            // Accumulate gradients over the batch.
            var dW = new double[L][][];
            var dB = new double[L][];
            for (int l = 1; l < L; l++)
            {
                dW[l] = new double[_layers[l]][];
                dB[l] = new double[_layers[l]];
                for (int j = 0; j < _layers[l]; j++)
                    dW[l][j] = new double[_layers[l - 1]];
            }

            double totalLoss = 0.0;

            for (int n = 0; n < N; n++)
            {
                var a = Forward(inputs[n]);

                // MSE loss: 0.5 * sum((a_out - t)^2)
                double[] t = new double[_layers[L - 1]];
                t[0] = targets[n];  // single output assumption
                for (int j = 0; j < _layers[L - 1]; j++)
                    totalLoss += 0.5 * Math.Pow(a[L - 1][j] - t[j], 2);

                // Backpropagation — compute deltas layer by layer from output.
                var delta = new double[L][];
                for (int l = 0; l < L; l++)
                    delta[l] = new double[_layers[l]];

                // Output layer delta: (a - t) * sigmoid'(a)
                for (int j = 0; j < _layers[L - 1]; j++)
                    delta[L - 1][j] = (a[L - 1][j] - t[j]) * SigmoidDerivative(a[L - 1][j]);

                // Hidden layer deltas: backprop chain rule
                for (int l = L - 2; l >= 1; l--)
                {
                    for (int k = 0; k < _layers[l]; k++)
                    {
                        double sum = 0.0;
                        for (int j = 0; j < _layers[l + 1]; j++)
                            sum += _w[l + 1][j][k] * delta[l + 1][j];
                        delta[l][k] = sum * SigmoidDerivative(a[l][k]);
                    }
                }

                // Accumulate gradients
                for (int l = 1; l < L; l++)
                {
                    for (int j = 0; j < _layers[l]; j++)
                    {
                        dB[l][j] += delta[l][j];
                        for (int k = 0; k < _layers[l - 1]; k++)
                            dW[l][j][k] += delta[l][j] * a[l - 1][k];
                    }
                }
            }

            // Apply averaged gradients
            for (int l = 1; l < L; l++)
            {
                for (int j = 0; j < _layers[l]; j++)
                {
                    _b[l][j] -= _lr * dB[l][j] / N;
                    for (int k = 0; k < _layers[l - 1]; k++)
                        _w[l][j][k] -= _lr * dW[l][j][k] / N;
                }
            }

            if (epoch % _logEvery == 0 || epoch == _epochs)
                LossHistory.Add((epoch, totalLoss / N));
        }
    }

    /// <summary>Predict a single output for the given input vector.</summary>
    public double Predict(double[] input)
    {
        var a = Forward(input);
        return a[_layers.Length - 1][0];
    }

    // --- Helpers --------------------------------------------------------------

    private static double Sigmoid(double x) => 1.0 / (1.0 + Math.Exp(-x));

    private static double SigmoidDerivative(double sigmoidOutput)
        => sigmoidOutput * (1.0 - sigmoidOutput);
}
