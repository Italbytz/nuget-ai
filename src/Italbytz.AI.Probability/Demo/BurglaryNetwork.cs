using System.Collections.Generic;

namespace Italbytz.AI.Probability.Bayes;

/// <summary>
/// The classic Burglary-Alarm Bayesian network from AIMA3e Fig. 14.2.
/// Query: P(Burglary | JohnCalls=true, MaryCalls=true) ≈ [0.284, 0.716]
/// </summary>
public static class BurglaryNetwork
{
    public static readonly IRandomVariable Burglary =
        new RandomVariable("Burglary", BooleanDomain.Instance);
    public static readonly IRandomVariable Earthquake =
        new RandomVariable("Earthquake", BooleanDomain.Instance);
    public static readonly IRandomVariable Alarm =
        new RandomVariable("Alarm", BooleanDomain.Instance);
    public static readonly IRandomVariable JohnCalls =
        new RandomVariable("JohnCalls", BooleanDomain.Instance);
    public static readonly IRandomVariable MaryCalls =
        new RandomVariable("MaryCalls", BooleanDomain.Instance);

    public static IBayesianNetwork Build()
    {
        var burglaryNode = new BayesNode(Burglary, new List<IBayesNode>(),
            new ConditionalProbabilityTable(Burglary, new IRandomVariable[0],
                new[] { 0.001, 0.999 }));

        var earthquakeNode = new BayesNode(Earthquake, new List<IBayesNode>(),
            new ConditionalProbabilityTable(Earthquake, new IRandomVariable[0],
                new[] { 0.002, 0.998 }));

        // P(Alarm | Burglary, Earthquake)
        // B=T,E=T | B=T,E=F | B=F,E=T | B=F,E=F
        var alarmNode = new BayesNode(Alarm, new List<IBayesNode> { burglaryNode, earthquakeNode },
            new ConditionalProbabilityTable(Alarm, new[] { Burglary, Earthquake },
                new[]
                {
                    0.95, 0.05,   // B=T, E=T
                    0.94, 0.06,   // B=T, E=F
                    0.29, 0.71,   // B=F, E=T
                    0.001, 0.999  // B=F, E=F
                }));

        // P(JohnCalls | Alarm)
        var johnNode = new BayesNode(JohnCalls, new List<IBayesNode> { alarmNode },
            new ConditionalProbabilityTable(JohnCalls, new[] { Alarm },
                new[]
                {
                    0.90, 0.10,  // A=T
                    0.05, 0.95   // A=F
                }));

        // P(MaryCalls | Alarm)
        var maryNode = new BayesNode(MaryCalls, new List<IBayesNode> { alarmNode },
            new ConditionalProbabilityTable(MaryCalls, new[] { Alarm },
                new[]
                {
                    0.70, 0.30,  // A=T
                    0.01, 0.99   // A=F
                }));

        return new BayesianNetwork(new List<IBayesNode>
        {
            burglaryNode, earthquakeNode, alarmNode, johnNode, maryNode
        });
    }
}
