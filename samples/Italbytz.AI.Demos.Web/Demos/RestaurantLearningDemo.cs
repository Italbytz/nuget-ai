using Italbytz.AI.Learning.Framework;
using Italbytz.AI.Learning.Learners;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record RestaurantPredictionRow(
    int Index,
    string Actual,
    string Id3Prediction,
    string CartPrediction);

internal sealed record RestaurantLearnerSummary(
    string Name,
    int Correct,
    int Incorrect,
    double Accuracy);

internal sealed record RestaurantLearningComparison(
    RestaurantLearnerSummary Id3,
    RestaurantLearnerSummary Cart,
    IReadOnlyList<RestaurantPredictionRow> Rows,
    string Summary);

internal static class RestaurantLearningDemo
{
    public static RestaurantLearningComparison Build()
    {
        var dataset = RestaurantDataSetFactory.Create();

        var id3Learner = new DecisionTreeLearner();
        id3Learner.Train(dataset);
        var id3Results = id3Learner.Test(dataset);
        var id3Predictions = id3Learner.Predict(dataset);

        var cartLearner = new CartDecisionTreeLearner();
        cartLearner.Train(dataset);
        var cartResults = cartLearner.Test(dataset);
        var cartPredictions = cartLearner.Predict(dataset);

        var rows = dataset.Examples
            .Select((example, index) => new RestaurantPredictionRow(
                index + 1,
                example.TargetValue(),
                id3Predictions[index],
                cartPredictions[index]))
            .ToArray();

        var id3Summary = BuildSummary("ID3 (information gain)", id3Results, dataset.Examples.Count);
        var cartSummary = BuildSummary("CART-style (gini)", cartResults, dataset.Examples.Count);

        var summary = BuildSummaryText(id3Summary, cartSummary);

        return new RestaurantLearningComparison(id3Summary, cartSummary, rows, summary);
    }

    private static RestaurantLearnerSummary BuildSummary(string name, int[] results, int total)
    {
        var correct = results[0];
        var incorrect = results[1];
        var accuracy = total == 0 ? 0.0 : (double)correct / total;
        return new RestaurantLearnerSummary(name, correct, incorrect, accuracy);
    }

    private static string BuildSummaryText(RestaurantLearnerSummary id3, RestaurantLearnerSummary cart)
    {
        if (Math.Abs(id3.Accuracy - cart.Accuracy) < 1e-12)
        {
            return "Both learners reach the same training accuracy on the canonical restaurant dataset. Use this view to compare split criteria, not just top-line score.";
        }

        var winner = id3.Accuracy > cart.Accuracy ? id3.Name : cart.Name;
        return $"{winner} reaches higher training accuracy on this dataset.";
    }
}
