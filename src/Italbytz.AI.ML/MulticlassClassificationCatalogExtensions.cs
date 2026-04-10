using System.Globalization;
using System.Text;
using Italbytz.AI.ML.Core;
using Microsoft.ML;

namespace Italbytz.AI.ML;

public static class MulticlassClassificationCatalogExtensions
{
    public static string GetPermutationFeatureImportanceTable(
        this MulticlassClassificationCatalog catalog,
        ITransformer model,
        IDataView data,
        string? labelColumnName,
        Metric metric)
    {
        StringBuilder sb = new();
        sb.AppendLine("Feature, Importance");

        System.Collections.Immutable.ImmutableDictionary<string, Microsoft.ML.Data.MulticlassClassificationMetricsStatistics> permutationFeatureImportance;
        try
        {
            permutationFeatureImportance = catalog.PermutationFeatureImportance(
                model,
                data,
                labelColumnName: labelColumnName);
        }
        catch (ArgumentNullException exception)
        {
            throw new InvalidOperationException(
                "Permutation feature importance requires a prediction-compatible ML.NET model with an accessible predictor.",
                exception);
        }

        foreach (var entry in permutationFeatureImportance)
        {
            var featureName = entry.Key;
            var value = entry.Value;
            var metricValue = metric switch
            {
                Metric.MacroAccuracy => value.MacroAccuracy.Mean,
                Metric.MicroAccuracy => value.MicroAccuracy.Mean,
                Metric.LogLoss => value.LogLoss.Mean,
                _ => 0.0
            };

            if (metricValue == 0.0)
            {
                continue;
            }

            var importance = metricValue * -1;
            sb.AppendLine($"{featureName}, {importance.ToString(CultureInfo.InvariantCulture)}");
        }

        return sb.ToString();
    }
}
