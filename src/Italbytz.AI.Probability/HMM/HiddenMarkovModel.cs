using System.Collections.Generic;

namespace Italbytz.AI.Probability.HMM;

/// <summary>
/// A Hidden Markov Model backed by an explicit transition matrix and a dictionary of
/// per-observation sensor distributions (AIMA3e p. 566).
/// </summary>
public class HiddenMarkovModel : IHiddenMarkovModel
{
    private readonly Dictionary<object, double[]> _sensorModels;

    public IRandomVariable StateVariable { get; }
    public int NumStates { get; }
    public double[,] TransitionMatrix { get; }
    public double[] Prior { get; }

    public HiddenMarkovModel(
        IRandomVariable stateVariable,
        double[,] transitionMatrix,
        double[] prior,
        Dictionary<object, double[]> sensorModels)
    {
        StateVariable = stateVariable;
        TransitionMatrix = transitionMatrix;
        Prior = prior;
        NumStates = stateVariable.Domain.Size;
        _sensorModels = sensorModels;
    }

    public double[] GetSensorDistribution(object observation) => _sensorModels[observation];
}
