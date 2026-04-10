using System.Globalization;
using System.Reflection;
using System.Text;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.Core.Configuration;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML;

public class Explainer(
    ITransformer model,
    IDataView data,
    ScenarioType scenario,
    string labelColumnName = DefaultColumnNames.Label)
{
    public string GetPermutationFeatureImportanceTable(Metric metric)
    {
        if (scenario != ScenarioType.Classification)
        {
            throw new InvalidOperationException(
                "Permutation feature importance is currently only supported for classification scenarios.");
        }

        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var scoredDataView = model.Transform(data);
        var effectiveLabelColumnName = scoredDataView.Schema.GetColumnOrNull(DefaultColumnNames.Label) != null
            ? DefaultColumnNames.Label
            : labelColumnName;

        return mlContext.MulticlassClassification.GetPermutationFeatureImportanceTable(
            model,
            scoredDataView,
            effectiveLabelColumnName,
            metric);
    }

    public string GetCeterisParibusTable<ModelInput, ModelOutput>(int featureIndex = 0, int gridCells = 100)
        where ModelInput : class, new()
        where ModelOutput : class, new()
    {
        if (scenario != ScenarioType.Classification)
        {
            throw new InvalidOperationException(
                "Ceteris Paribus is currently only supported for classification scenarios.");
        }

        if (featureIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(featureIndex), "Feature index must be non-negative.");
        }

        var mlContext = ThreadSafeMLContext.LocalMLContext;
        var predictionEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(model);
        var features = data.GetFeatures<ModelInput>(labelColumnName);
        if (features.Count == 0)
        {
            throw new InvalidOperationException(
                "No features found in the provided data. Ensure that the data contains valid numerical features for analysis.");
        }

        var selectedFeature = features.FirstOrDefault(feature => feature.ColumnIndex == featureIndex) ??
                              features.ElementAtOrDefault(featureIndex) ??
                              throw new ArgumentException($"No feature found for index {featureIndex}.", nameof(featureIndex));

        if (selectedFeature is not NumericalFeature numericalFeature || numericalFeature.ValueRange.Count == 0)
        {
            throw new ArgumentException(
                "Ceteris Paribus is currently only supported for numerical features.",
                nameof(featureIndex));
        }

        gridCells = Math.Max(gridCells, 2);
        var minValue = numericalFeature.ValueRange.Min();
        var maxValue = numericalFeature.ValueRange.Max();
        var step = (maxValue - minValue) / (gridCells - 1);
        var gridValues = Enumerable.Range(0, gridCells)
            .Select(index => minValue + index * step)
            .ToArray();

        var rows = mlContext.Data.CreateEnumerable<ModelInput>(data, reuseRowObject: false).ToArray();
        StringBuilder sb = new();
        sb.AppendLine("Feature,Score,Class");

        foreach (var gridValue in gridValues)
        {
            List<float[]> scores = [];

            foreach (var row in rows)
            {
                var modifiedRow = new ModelInput();
                foreach (var property in typeof(ModelInput).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!property.CanWrite)
                    {
                        continue;
                    }

                    var value = string.Equals(property.Name, selectedFeature.PropertyName, StringComparison.OrdinalIgnoreCase)
                        ? gridValue
                        : property.GetValue(row);
                    property.SetValue(modifiedRow, value);
                }

                var prediction = predictionEngine.Predict(modifiedRow);
                scores.Add(ExtractScoreArray(prediction));
            }

            var scoreCount = scores.Count == 0 ? 0 : scores[0].Length;
            for (var index = 0; index < scoreCount; index++)
            {
                var averageScore = scores.Select(score => score[index]).Average();
                sb.AppendLine(
                    $"{gridValue.ToString(CultureInfo.InvariantCulture)},{averageScore.ToString(CultureInfo.InvariantCulture)},Class{index}");
            }
        }

        return sb.ToString();
    }

    private static float[] ExtractScoreArray(object prediction)
    {
        var scoreProperty = prediction.GetType().GetProperty("Score");
        var scoreValue = scoreProperty?.GetValue(prediction);

        return scoreValue switch
        {
            float[] scoreArray => scoreArray,
            VBuffer<float> scoreBuffer => scoreBuffer.GetValues().ToArray(),
            _ => throw new InvalidOperationException(
                "Prediction output does not contain a supported 'Score' property.")
        };
    }
}
