using Italbytz.AI.Robotics;

namespace Italbytz.AI.Tests;

[TestClass]
public class RoboticsTests
{
    // Minimal 2-D pose for a hallway: just an x position.
    private record Pose1D(double X) : IPose<Pose1D>
    {
        public Pose1D ApplyMove(IMove<Pose1D> move) => move.GenerateNoisySample(this);
    }

    // Deterministic forward-step move (+ noise handled elsewhere)
    private class ForwardMove : IMove<Pose1D>
    {
        private readonly double _delta;
        private readonly double _noise;
        private readonly Random _rng;

        public ForwardMove(double delta, double noise = 0.05, int seed = 0)
        {
            _delta = delta;
            _noise = noise;
            _rng = new Random(seed);
        }

        public Pose1D GenerateNoisySample(Pose1D from) =>
            new Pose1D(from.X + _delta + (_rng.NextDouble() - 0.5) * _noise);
    }

    // Flat 1-D corridor [0, 10]
    private class Corridor : IMap<Pose1D>
    {
        public bool IsPoseValid(Pose1D pose) => pose.X >= 0 && pose.X <= 10;
        public double RayCast(Pose1D pose, double angle) => 10 - pose.X; // distance to right wall
    }

    private class GaussianReading : IRangeReading
    {
        public double Angle => 0;
        public double Range { get; }

        public GaussianReading(double range) => Range = range;

        public double CalculateWeight(double castDistance)
        {
            double sigma = 0.5;
            double diff = Range - castDistance;
            return Math.Exp(-0.5 * diff * diff / (sigma * sigma));
        }
    }

    [TestMethod]
    public void MCL_SmokeTest_ParticleCloudUpdates()
    {
        var map = new Corridor();
        var rng = new Random(1);

        // Generate initial uniform cloud
        var initialCloud = Enumerable.Range(0, 200)
            .Select(_ => new Pose1D(rng.NextDouble() * 10.0))
            .ToList();

        IReadOnlyList<Pose1D> CloudGen() => initialCloud;

        var mcl = new MonteCarloLocalization<Pose1D>(
            map, CloudGen, lowWeightThreshold: 0.0, seed: 42);

        var move = new ForwardMove(delta: 0.5, seed: 2);
        var readings = new[] { new GaussianReading(range: 7.0) }; // robot is ~3 units in

        var updated = mcl.Localize(initialCloud, move, readings);

        Assert.IsNotNull(updated);
        Assert.HasCount(initialCloud.Count, updated);
        // All particles should be valid poses
        foreach (var p in updated)
            Assert.IsTrue(map.IsPoseValid(p), $"Invalid pose: {p.X}");
    }
}
