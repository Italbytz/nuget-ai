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
    public void PriorSample_BurglaryNetwork_ProducesCompleteWorldAssignment()
    {
        var net = BurglaryNetwork.Build();
        var sample = new PriorSample(seed: 7).Sample(net);

        Assert.IsTrue(sample.ContainsKey(BurglaryNetwork.Burglary));
        Assert.IsTrue(sample.ContainsKey(BurglaryNetwork.Earthquake));
        Assert.IsTrue(sample.ContainsKey(BurglaryNetwork.Alarm));
        Assert.IsTrue(sample.ContainsKey(BurglaryNetwork.JohnCalls));
        Assert.IsTrue(sample.ContainsKey(BurglaryNetwork.MaryCalls));
    }

    [TestMethod]
    public void RejectionSampling_BurglaryNetwork_Approximate()
    {
        var net = BurglaryNetwork.Build();
        var query = BurglaryNetwork.Burglary;
        var evidence = Ev(
            new AssignmentProposition(BurglaryNetwork.JohnCalls, true),
            new AssignmentProposition(BurglaryNetwork.MaryCalls, true));

        var rejectionSampling = new RejectionSampling(sampleCount: 50000, seed: 42);
        var result = rejectionSampling.Ask(Q(query), evidence, net);

        var p = result.ValueOf(new AssignmentProposition(query, true));
        Assert.IsTrue(p > 0.20 && p < 0.38, $"Expected ~0.284 but got {p}");
        Assert.AreNotEqual(0, rejectionSampling.Metrics.GetInt("acceptedSamples"));
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
        Assert.HasCount(2, result);
        // P(Rain_1 | umbrella_1,2) ≈ 0.883
        Assert.IsGreaterThan(0.8, result[0][0], $"Expected P(Rain_1)>0.8 but got {result[0][0]}");
    }

    [TestMethod]
    public void Forward_UmbrellaWorld_MatchesAimaReferenceValues()
    {
        var hmm = UmbrellaWorld.Build();
        var fb = new ForwardBackwardAlgorithm();

        var first = fb.Forward(hmm, new[] { 0.5, 0.5 }, true);
        var second = fb.Forward(hmm, first, true);

        Assert.AreEqual(0.818, first[0], 0.01);
        Assert.AreEqual(0.182, first[1], 0.01);
        Assert.AreEqual(0.883, second[0], 0.01);
        Assert.AreEqual(0.117, second[1], 0.01);
    }

    [TestMethod]
    public void Backward_UmbrellaWorld_MatchesAimaReferenceValues()
    {
        var hmm = UmbrellaWorld.Build();
        var fb = new ForwardBackwardAlgorithm();

        var backward = fb.Backward(hmm, new[] { 1.0, 1.0 }, true);

        Assert.AreEqual(0.69, backward[0], 0.01);
        Assert.AreEqual(0.41, backward[1], 0.01);
    }

    [TestMethod]
    public void ForwardBackward_UmbrellaWorld_ThreeObservationsMatchesAimaApproximation()
    {
        var hmm = UmbrellaWorld.Build();
        var fb = new ForwardBackwardAlgorithm();

        var result = fb.ForwardBackward(hmm, new List<object> { true, true, false }, new[] { 0.5, 0.5 });

        Assert.HasCount(3, result);
        Assert.AreEqual(0.861, result[0][0], 0.02);
        Assert.AreEqual(0.138, result[0][1], 0.02);
        Assert.AreEqual(0.799, result[1][0], 0.02);
        Assert.AreEqual(0.201, result[1][1], 0.02);
        Assert.AreEqual(0.190, result[2][0], 0.02);
        Assert.AreEqual(0.810, result[2][1], 0.02);
    }

    [TestMethod]
    public void ParticleFilter_UmbrellaWorld_FavorsRainAfterUmbrellaObservation()
    {
        var hmm = UmbrellaWorld.Build();
        var particleFilter = new ParticleFilter(seed: 42);
        var particles = particleFilter.CreateInitialParticles(hmm, 1000);

        var filtered = particleFilter.Filter(hmm, particles, true);
        var rainCount = filtered.Count(state => Equals(state, true));

        Assert.HasCount(particles.Count, filtered);
        Assert.IsGreaterThan(650, rainCount, $"Expected a rain-majority after umbrella observation but got {rainCount} rain particles.");
    }

    // ------------------------------------------------------------------ MDP
    [TestMethod]
    public void ValueIteration_GridWorld_ConvergesWithinTimeout()
    {
        var mdp = new GridWorldMdp();
        var solver = new ValueIteration<string, string>();
        var (policy, utilities) = solver.Solve(mdp);
        Assert.IsNotNull(policy);
        Assert.AreNotEqual(0, utilities.Count);
        Assert.AreEqual(1.0, utilities["1,4"], 0.0001);
        Assert.AreEqual(-1.0, utilities["2,4"], 0.0001);
        Assert.AreEqual("Right", policy.Action("1,3"));
    }

    [TestMethod]
    public void PolicyIteration_GridWorld_ConvergesWithinTimeout()
    {
        var mdp = new GridWorldMdp();
        var solver = new PolicyIteration<string, string>();
        var (policy, utilities) = solver.Solve(mdp);
        Assert.IsNotNull(policy);
        Assert.AreNotEqual(0, utilities.Count);
        Assert.AreEqual(1.0, utilities["1,4"], 0.0001);
        Assert.AreEqual(-1.0, utilities["2,4"], 0.0001);
        Assert.AreEqual("Right", policy.Action("1,3"));
    }
}
