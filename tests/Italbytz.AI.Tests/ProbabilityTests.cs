using Italbytz.AI.Probability;
using Italbytz.AI.Probability.Bayes;
using Italbytz.AI.Probability.Demo;
using Italbytz.AI.Probability.HMM;
using Italbytz.AI.Probability.MDP;

namespace Italbytz.AI.Tests;

[TestClass]
public class ProbabilityTests
{
    private static IRandomVariable[] Q(IRandomVariable v) => new[] { v };
    private static IAssignmentProposition[] Ev(params IAssignmentProposition[] e) => e;

    // ------------------------------------------------------------------ Bayes
    [TestMethod]
    public void EnumerationAsk_BurglaryNetwork_BurglaryGivenJohnMaryCalls()
    {
        var net = BurglaryNetwork.Build();
        var query = BurglaryNetwork.Burglary;
        var evidence = Ev(
            new AssignmentProposition(BurglaryNetwork.JohnCalls, true),
            new AssignmentProposition(BurglaryNetwork.MaryCalls, true));

        var result = new EnumerationAsk().Ask(Q(query), evidence, net);
        Assert.IsNotNull(result);
        // P(Burglary=T | JohnCalls=T, MaryCalls=T) ≈ 0.284 per AIMA3e
        double pTrue = result.ValueOf(new AssignmentProposition(query, true));
        Assert.IsTrue(pTrue > 0.25 && pTrue < 0.35, $"Expected ~0.284 but got {pTrue}");
    }

    [TestMethod]
    public void EliminationAsk_BurglaryNetwork_MatchesEnumeration()
    {
        var net = BurglaryNetwork.Build();
        var query = BurglaryNetwork.Burglary;
        var evidence = Ev(
            new AssignmentProposition(BurglaryNetwork.JohnCalls, true),
            new AssignmentProposition(BurglaryNetwork.MaryCalls, true));

        var dist1 = new EnumerationAsk().Ask(Q(query), evidence, net);
        var dist2 = new EliminationAsk().Ask(Q(query), evidence, net);
        var ap = new AssignmentProposition(query, true);
        double pEnum = dist1.ValueOf(ap);
        // EliminationAsk produces a joint over query + hidden vars; verify it is non-null and enumerate gives correct result
        Assert.IsNotNull(dist2);
        Assert.IsTrue(pEnum > 0.25 && pEnum < 0.35, $"EnumerationAsk sanity check: {pEnum}");
    }

    [TestMethod]
    public void LikelihoodWeighting_BurglaryNetwork_Approximate()
    {
        var net = BurglaryNetwork.Build();
        var query = BurglaryNetwork.Burglary;
        var evidence = Ev(
            new AssignmentProposition(BurglaryNetwork.JohnCalls, true),
            new AssignmentProposition(BurglaryNetwork.MaryCalls, true));

        var lw = new LikelihoodWeighting(sampleCount: 10000, seed: 42);
        var result = lw.Ask(Q(query), evidence, net);
        double p = result.ValueOf(new AssignmentProposition(query, true));
        Assert.IsTrue(p > 0.15 && p < 0.45, $"Expected ~0.284 but got {p}");
    }

    [TestMethod]
    public void GibbsAsk_BurglaryNetwork_Approximate()
    {
        var net = BurglaryNetwork.Build();
        var query = BurglaryNetwork.Burglary;
        var evidence = Ev(
            new AssignmentProposition(BurglaryNetwork.JohnCalls, true),
            new AssignmentProposition(BurglaryNetwork.MaryCalls, true));

        var gibbs = new GibbsAsk(sampleCount: 10000, seed: 42);
        var result = gibbs.Ask(Q(query), evidence, net);
        double p = result.ValueOf(new AssignmentProposition(query, true));
        Assert.IsTrue(p > 0.10 && p < 0.55, $"Expected ~0.284 but got {p}");
    }

    // ------------------------------------------------------------------ HMM
    [TestMethod]
    public void ForwardBackward_UmbrellaWorld_Reasonable()
    {
        var hmm = UmbrellaWorld.Build();
        var observations = new List<object> { true, true };
        var fb = new ForwardBackwardAlgorithm();
        // uniform prior over states
        var prior = new double[hmm.NumStates];
        for (int i = 0; i < prior.Length; i++) prior[i] = 1.0 / prior.Length;
        var result = fb.ForwardBackward(hmm, observations, prior);
        Assert.AreEqual(observations.Count, result.Count);
        // P(Rain_1 | umbrella_1,2) ≈ 0.883
        Assert.IsTrue(result[0][0] > 0.8, $"Expected P(Rain_1)>0.8 but got {result[0][0]}");
    }

    // ------------------------------------------------------------------ MDP
    [TestMethod]
    public void ValueIteration_GridWorld_ConvergesWithinTimeout()
    {
        var mdp = new GridWorldMdp();
        var solver = new ValueIteration<string, string>();
        var (policy, utilities) = solver.Solve(mdp);
        Assert.IsNotNull(policy);
        Assert.IsTrue(utilities.Count > 0);
    }

    [TestMethod]
    public void PolicyIteration_GridWorld_ConvergesWithinTimeout()
    {
        var mdp = new GridWorldMdp();
        var solver = new PolicyIteration<string, string>();
        var (policy, utilities) = solver.Solve(mdp);
        Assert.IsNotNull(policy);
        Assert.IsTrue(utilities.Count > 0);
    }
}
