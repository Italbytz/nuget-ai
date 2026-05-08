using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Italbytz.AI.Learning.Framework;
using Italbytz.AI.Learning.Learners;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record IrisClusterRow(
    int Index,
    string SepalLength,
    string SepalWidth,
    string PetalLength,
    string PetalWidth,
    string TrueSpecies,
    string ClusterId);

internal sealed record IrisCentroid(
    int ClusterId,
    string SepalLength,
    string SepalWidth,
    string PetalLength,
    string PetalWidth,
    string DominantSpecies);

internal sealed record IrisClusteringResult(
    IReadOnlyList<IrisClusterRow> Rows,
    IReadOnlyList<IrisCentroid> Centroids,
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
                example.GetAttributeValueAsString("sepal_length"),
                example.GetAttributeValueAsString("sepal_width"),
                example.GetAttributeValueAsString("petal_length"),
                example.GetAttributeValueAsString("petal_width"),
                example.TargetValue(),
                predictions[i]))
            .ToArray();

        // Determine dominant species per cluster
        var centroids = learner.Centroids
            .Select((c, idx) =>
            {
                var clusterLabel = idx.ToString(CultureInfo.InvariantCulture);
                var dominant = rows
                    .Where(r => r.ClusterId == clusterLabel)
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

        var summary = $"k-Means (k=3) on 30 Iris examples. " +
                      $"Purity: {correct}/{total} ({purity:P0}).";

        return new IrisClusteringResult(rows, centroids, correct, total, purity, summary);
    }
}
