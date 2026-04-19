using System.Collections.Generic;

namespace Italbytz.AI.Probability.HMM;

/// <summary>
/// The Umbrella World HMM from AIMA3e Chapter 15.
/// States: Rain ∈ {true, false}.
/// Observations: Umbrella ∈ {true, false}.
/// T[i,j] = P(X_t = j | X_{t-1} = i); index 0 = Rain=true, 1 = Rain=false.
/// Sensor: P(Umbrella=true | Rain=true) = 0.9, P(Umbrella=true | Rain=false) = 0.2.
/// </summary>
public static class UmbrellaWorld
{
    public static readonly IRandomVariable Rain =
        new RandomVariable("Rain", BooleanDomain.Instance);

    public static IHiddenMarkovModel Build()
    {
        var transition = new double[,]
        {
            { 0.7, 0.3 },  // Rain=T → Rain=T, Rain=T → Rain=F
            { 0.3, 0.7 }   // Rain=F → Rain=T, Rain=F → Rain=F
        };

        var prior = new[] { 0.5, 0.5 };

        // P(Umbrella | Rain): sensorModels[observation] = [P(obs|Rain=T), P(obs|Rain=F)]
        var sensorModels = new Dictionary<object, double[]>
        {
            { true,  new[] { 0.9, 0.2 } },  // umbrella visible
            { false, new[] { 0.1, 0.8 } }   // no umbrella
        };

        return new HiddenMarkovModel(Rain, transition, prior, sensorModels);
    }
}
