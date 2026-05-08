using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Italbytz.AI.Learning.Framework;
using Italbytz.AI.Learning.Learners;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record TaxiPredictionRow(
    int Index,
    string PassengerCount,
    string TripTimeSecs,
    string TripDistance,
    string ActualFare,
    string PredictedFare,
    bool WithinTolerance);

internal sealed record TaxiRegressionResult(
    IReadOnlyList<TaxiPredictionRow> Rows,
    double RSquared,
    double Mae,
    int WithinTolerance,
    int Total,
    string Equation,
    string Summary);

internal static class TaxiRegressionDemo
{
    public static TaxiRegressionResult Build()
    {
        var dataset = TaxiDataSetFactory.Create();

        var learner = new LinearRegressionLearner();
        learner.Train(dataset);

        var predictions = learner.Predict(dataset);
        var results = learner.Test(dataset);
        var r2 = learner.RSquared(dataset);
        var mae = learner.MeanAbsoluteError(dataset);

        var rows = dataset.Examples
            .Select((example, i) =>
            {
                var actual = example.GetAttributeValueAsString("fare_amount");
                var predicted = predictions[i];
                var withinTolerance = System.Math.Abs(
                    double.Parse(predicted, CultureInfo.InvariantCulture) -
                    double.Parse(actual, CultureInfo.InvariantCulture)) <= 2.0;

                return new TaxiPredictionRow(
                    i + 1,
                    example.GetAttributeValueAsString("passenger_count"),
                    example.GetAttributeValueAsString("trip_time_in_secs"),
                    example.GetAttributeValueAsString("trip_distance"),
                    actual,
                    predicted,
                    withinTolerance);
            })
            .ToArray();

        var coeff = learner.Coefficients;
        var featureNames = learner.FeatureNames;
        var equation = BuildEquationString(coeff, featureNames);

        var summary = $"Linear regression on {dataset.Examples.Count} taxi trips. " +
                      $"R² = {r2:F3}, MAE = {mae:F2} USD.";

        return new TaxiRegressionResult(
            rows, r2, mae, results[0], dataset.Examples.Count, equation, summary);
    }

    private static string BuildEquationString(double[] coeff, string[] features)
    {
        var parts = new List<string>
        {
            coeff[0].ToString("F2", CultureInfo.InvariantCulture)
        };
        for (var i = 0; i < features.Length; i++)
        {
            var sign = coeff[i + 1] >= 0 ? "+ " : "− ";
            parts.Add($"{sign}{System.Math.Abs(coeff[i + 1]).ToString("F4", CultureInfo.InvariantCulture)} × {features[i]}");
        }
        return "fare = " + string.Join(" ", parts);
    }
}
