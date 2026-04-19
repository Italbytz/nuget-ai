using System.Collections.Generic;

namespace Italbytz.AI.Probability;

/// <summary>
/// A Hidden Markov Model with a single discrete state variable, a transition
/// matrix, and an observation (sensor) model (AIMA3e p. 566).
/// </summary>
public interface IHiddenMarkovModel
{
    IRandomVariable StateVariable { get; }

    int NumStates { get; }

    /// <summary>
    /// Transition matrix T where T[i,j] = P(X_t = j | X_{t-1} = i).
    /// </summary>
    double[,] TransitionMatrix { get; }

    double[] Prior { get; }

    /// <summary>
    /// Returns P(observation | X = state_i) for each state index i.
    /// </summary>
    double[] GetSensorDistribution(object observation);
}
