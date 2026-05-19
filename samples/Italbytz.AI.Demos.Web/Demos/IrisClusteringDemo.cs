using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Italbytz.AI.Learning.Framework;
using Italbytz.AI.Learning.Learners;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record IrisClusterRow(
    int Index,
    double SepalLength,
    double SepalWidth,
    double PetalLength,
    double PetalWidth,
    string TrueSpecies,
    int ClusterId);

internal sealed record IrisCentroid(
    int ClusterId,
    string SepalLength,
    string SepalWidth,
    string PetalLength,
    string PetalWidth,
    string DominantSpecies);

internal sealed record IrisCentroidPoint(
    int ClusterId,
    double SepalLength,
    double SepalWidth,
    double PetalLength,
    double PetalWidth);

internal sealed record IrisCentroidFrame(
    int Iteration,
    IReadOnlyList<IrisCentroidPoint> Centroids);

internal sealed record IrisClusteringResult(
    IReadOnlyList<IrisClusterRow> Rows,
    IReadOnlyList<IrisCentroid> Centroids,
    IReadOnlyList<IrisCentroidFrame> CentroidFrames,
    int Correct,
    int Total,
    double Purity,
    string Summary);

internal static class IrisClusteringDemo
{
    public static IrisClusteringResult Build()
    {
        var dataset = IrisDataSetFactory.Create();

        var learner = new KMeansLearner(k: 3);
        learner.Train(dataset);

        var predictions = learner.Predict(dataset);
        var results = learner.Test(dataset);
        var correct = results[0];
        var total = dataset.Examples.Count;
        var purity = total == 0 ? 0.0 : (double)correct / total;

        var rows = dataset.Examples
            .Select((example, i) => new IrisClusterRow(
                i + 1,
                double.Parse(example.GetAttributeValueAsString("sepal_length"), CultureInfo.InvariantCulture),
                double.Parse(example.GetAttributeValueAsString("sepal_width"), CultureInfo.InvariantCulture),
                double.Parse(example.GetAttributeValueAsString("petal_length"), CultureInfo.InvariantCulture),
                double.Parse(example.GetAttributeValueAsString("petal_width"), CultureInfo.InvariantCulture),
                example.TargetValue(),
                int.Parse(predictions[i], CultureInfo.InvariantCulture)))
            .ToArray();

        // Determine dominant species per cluster
        var centroids = learner.Centroids
            .Select((c, idx) =>
            {
                var dominant = rows
                    .Where(r => r.ClusterId == idx)
                    .GroupBy(r => r.TrueSpecies)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefault("—");

                return new IrisCentroid(
                    idx,
                    c[0].ToString("F2", CultureInfo.InvariantCulture),
                    c[1].ToString("F2", CultureInfo.InvariantCulture),
                    c[2].ToString("F2", CultureInfo.InvariantCulture),
                    c[3].ToString("F2", CultureInfo.InvariantCulture),
                    dominant);
            })
            .ToArray();

        var centroidFrames = BuildCentroidFrames(rows, k: 3, maxIterations: 12);

        var summary = $"k-Means (k=3) on 30 Iris examples. " +
                      $"Purity: {correct}/{total} ({purity:P0}).";

        return new IrisClusteringResult(rows, centroids, centroidFrames, correct, total, purity, summary);
    }

    private static IReadOnlyList<IrisCentroidFrame> BuildCentroidFrames(
        IReadOnlyList<IrisClusterRow> rows,
        int k,
        int maxIterations)
    {
        if (rows.Count == 0)
        {
            return [];
        }

        static double DistanceSquared(double[] a, double[] b)
            => a.Zip(b, (x, y) => (x - y) * (x - y)).Sum();

        var random = new Random(42);
        var points = rows
            .Select(r => new[] { r.SepalLength, r.SepalWidth, r.PetalLength, r.PetalWidth })
            .ToArray();

        var centroidSeeds = Enumerable.Range(0, points.Length)
            .OrderBy(_ => random.Next())
            .Take(k)
            .ToArray();

        var centroids = centroidSeeds
            .Select(i => (double[])points[i].Clone())
            .ToArray();

        var assignments = new int[points.Length];
        var frames = new List<IrisCentroidFrame>
        {
            ToFrame(0, centroids)
        };

        for (var iteration = 1; iteration <= maxIterations; iteration++)
        {
            var changed = false;
            for (var i = 0; i < points.Length; i++)
            {
                var nearest = 0;
                var minDist = double.MaxValue;
                for (var c = 0; c < k; c++)
                {
                    var dist = DistanceSquared(points[i], centroids[c]);
                    if (dist >= minDist)
                    {
                        continue;
                    }

                    minDist = dist;
                    nearest = c;
                }

                if (nearest == assignments[i])
                {
                    continue;
                }

                assignments[i] = nearest;
                changed = true;
            }

            for (var c = 0; c < k; c++)
            {
                var members = points
                    .Where((_, i) => assignments[i] == c)
                    .ToArray();

                if (members.Length == 0)
                {
                    continue;
                }

                for (var d = 0; d < centroids[c].Length; d++)
                {
                    centroids[c][d] = members.Average(p => p[d]);
                }
            }

            frames.Add(ToFrame(iteration, centroids));

            if (!changed)
            {
                break;
            }
        }

        return frames;

        static IrisCentroidFrame ToFrame(int iteration, IReadOnlyList<double[]> centroidVectors)
            => new(
                iteration,
                centroidVectors
                    .Select((c, idx) => new IrisCentroidPoint(idx, c[0], c[1], c[2], c[3]))
                    .ToArray());
    }
}
