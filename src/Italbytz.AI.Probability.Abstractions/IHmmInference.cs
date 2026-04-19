using System.Collections.Generic;

namespace Italbytz.AI.Probability;

/// <summary>
/// Temporal inference algorithms for Hidden Markov Models.
/// Covers filtering (forward), smoothing (forward-backward), and prediction (AIMA3e ch. 15).
/// </summary>
public interface IHmmInference
{
    /// <summary>
    /// One forward step: f_{t+1} = α · P(e_{t+1}|X) ⊙ (T^T · f_t).
    /// Returns the normalised forward vector after incorporating <paramref name="observation"/>.
    /// </summary>
    double[] Forward(IHiddenMarkovModel model, double[] previousForward, object observation);

    /// <summary>
    /// One backward step: b_k = T · P(e_{k+1}|X) ⊙ b_{k+1}.
    /// </summary>
    double[] Backward(IHiddenMarkovModel model, double[] nextBackward, object observation);

    /// <summary>
    /// FORWARD-BACKWARD algorithm (AIMA3e Fig. 15.4).
    /// Returns smoothed distributions P(X_k | e_{1:T}) for k = 1..T (0-indexed).
    /// </summary>
    IReadOnlyList<double[]> ForwardBackward(
        IHiddenMarkovModel model,
        IReadOnlyList<object> observations,
        double[] prior);
}
