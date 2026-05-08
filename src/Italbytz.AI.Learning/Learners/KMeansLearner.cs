using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Italbytz.AI.Learning.Learners;

public class KMeansLearner : ILearner
{
    private readonly int _k;
    private readonly int _maxIterations;
    private readonly Random _random;
    private double[][] _centroids = [];
    private string[] _featureNames = [];
    private string _targetName = string.Empty;

    public KMeansLearner(int k = 3, int maxIterations = 100, int seed = 42)
    {
        _k = k;
        _maxIterations = maxIterations;
        _random = new Random(seed);
    }

    /// <summary>Centroid coordinates after training, indexed by cluster id.</summary>
    public double[][] Centroids => _centroids;

    public string[] FeatureNames => _featureNames;

    public void Train(IDataSet ds)
    {
        _targetName = ds.Specification.TargetAttribute;
        _featureNames = ds.GetNonTargetAttributes().ToArray();

        var points = ExtractFeatures(ds);
        var n = points.Length;

        // Forgy initialisation: pick k distinct data points as initial centroids
        var indices = Enumerable.Range(0, n)
            .OrderBy(_ => _random.Next())
            .Take(_k)
            .ToArray();
        _centroids = indices.Select(i => (double[])points[i].Clone()).ToArray();

        var assignments = new int[n];

        for (var iter = 0; iter < _maxIterations; iter++)
        {
            var changed = false;

            for (var i = 0; i < n; i++)
            {
                var nearest = NearestCentroid(points[i]);
                if (nearest != assignments[i])
                {
                    assignments[i] = nearest;
                    changed = true;
                }
            }

            if (!changed)
                break;

            // Recompute centroids
            for (var c = 0; c < _k; c++)
            {
                var members = points
                    .Where((_, i) => assignments[i] == c)
                    .ToArray();

                if (members.Length == 0)
                    continue;

                for (var d = 0; d < _featureNames.Length; d++)
                    _centroids[c][d] = members.Average(p => p[d]);
            }
        }
    }

    public string Predict(IExample e)
    {
        var point = ExtractFeature(e);
        return NearestCentroid(point).ToString(CultureInfo.InvariantCulture);
    }

    public string[] Predict(IDataSet ds)
        => ds.Examples.Select(Predict).ToArray();

    /// <summary>
    /// Returns [purity-correct, purity-incorrect].
    /// Purity: for each cluster, count the majority true label; sum those counts.
    /// </summary>
    public int[] Test(IDataSet ds)
    {
        var predictions = Predict(ds);
        var trueLabels = ds.Examples.Select(e => e.TargetValue()).ToArray();

        var correct = 0;
        for (var c = 0; c < _k; c++)
        {
            var cLabel = c.ToString(CultureInfo.InvariantCulture);
            var clusterLabels = trueLabels
                .Where((_, i) => predictions[i] == cLabel)
                .ToArray();

            if (clusterLabels.Length == 0)
                continue;

            correct += clusterLabels
                .GroupBy(l => l)
                .Max(g => g.Count());
        }

        return [correct, trueLabels.Length - correct];
    }

    private int NearestCentroid(double[] point)
    {
        var minDist = double.MaxValue;
        var nearest = 0;

        for (var c = 0; c < _k; c++)
        {
            var dist = EuclideanSquared(point, _centroids[c]);
            if (dist >= minDist)
                continue;

            minDist = dist;
            nearest = c;
        }

        return nearest;
    }

    private static double EuclideanSquared(double[] a, double[] b)
        => a.Zip(b, (x, y) => (x - y) * (x - y)).Sum();

    private double[][] ExtractFeatures(IDataSet ds)
        => ds.Examples.Select(ExtractFeature).ToArray();

    private double[] ExtractFeature(IExample e)
        => _featureNames
            .Select(f => double.Parse(
                e.GetAttributeValueAsString(f),
                CultureInfo.InvariantCulture))
            .ToArray();
}
