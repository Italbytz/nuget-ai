using Italbytz.AI.Learning;
using Italbytz.AI.Learning.Framework;
using Italbytz.AI.Learning.Inductive;
using Italbytz.AI.Learning.Learners;
using Italbytz.Graph.Visualization;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record RestaurantPredictionRow(
    int Index,
    IReadOnlyList<string> FeatureValues,
    string Actual,
    string Id3Prediction,
    string CartPrediction);

internal sealed record RestaurantLearnerSummary(
    string Name,
    int Correct,
    int Incorrect,
    double Accuracy);

internal sealed record RestaurantDecisionPath(
    IReadOnlyList<string> NodeIds,
    IReadOnlyList<string> EdgeKeys,
    string? LeafNodeId);

internal sealed record RestaurantTreeView(
    GraphViewModel Graph,
    IReadOnlyDictionary<int, RestaurantDecisionPath> PathsByRow);

internal sealed record RestaurantLearningComparison(
    RestaurantLearnerSummary Id3,
    RestaurantLearnerSummary Cart,
    IReadOnlyList<string> FeatureNames,
    IReadOnlyList<RestaurantPredictionRow> Rows,
    RestaurantTreeView Id3Tree,
    RestaurantTreeView CartTree,
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
        var featureNames = dataset.GetNonTargetAttributes().ToArray();

        var rows = dataset.Examples
            .Select((example, index) => new RestaurantPredictionRow(
                index + 1,
                featureNames.Select(example.GetAttributeValueAsString).ToArray(),
                example.TargetValue(),
                id3Predictions[index],
                cartPredictions[index]))
            .ToArray();

        var id3Summary = BuildSummary("ID3 (information gain)", id3Results, dataset.Examples.Count);
        var cartSummary = BuildSummary("CART-style (gini)", cartResults, dataset.Examples.Count);
        var id3Tree = BuildTreeView(id3Learner.Tree, dataset);
        var cartTree = BuildTreeView(cartLearner.Tree, dataset);

        var summary = BuildSummaryText(id3Summary, cartSummary);

        return new RestaurantLearningComparison(id3Summary, cartSummary, featureNames, rows, id3Tree, cartTree, summary);
    }

    private static RestaurantTreeView BuildTreeView(DecisionTree? tree, IDataSet dataset)
    {
        if (tree is null)
        {
            return new RestaurantTreeView(GraphViewFactory.BuildTreeGraphView([]), new Dictionary<int, RestaurantDecisionPath>());
        }

        var nodes = new List<TreeLayoutNode>();
        var nodeIdsByReference = new Dictionary<DecisionTree, string>();
        var nextId = 0;
        BuildTreeLayoutNodes(tree, null, string.Empty, 0, nodes, nodeIdsByReference, ref nextId);

        var paths = dataset.Examples
            .Select((example, index) => new KeyValuePair<int, RestaurantDecisionPath>(index + 1, BuildDecisionPath(tree, example, nodeIdsByReference)))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        var graph = GraphViewFactory.BuildTreeGraphView(nodes, horizontalSpacing: 160.0, verticalSpacing: 120.0, nodeRadius: 32.0);

        return new RestaurantTreeView(graph, paths);
    }

    private static void BuildTreeLayoutNodes(
        DecisionTree node,
        string? parentId,
        string edgeLabel,
        int order,
        List<TreeLayoutNode> nodes,
        Dictionary<DecisionTree, string> nodeIdsByReference,
        ref int nextId)
    {
        var id = $"t{nextId++}";
        nodeIdsByReference[node] = id;
        var label = node switch
        {
            ConstantDecisionTree leaf => leaf.Value,
            _ => node.SplitAttribute ?? "?"
        };

        nodes.Add(new TreeLayoutNode(id, label, parentId, edgeLabel, false, order));

        if (node.IsLeaf)
        {
            return;
        }

        var childOrder = 0;
        foreach (var child in node.Children.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            BuildTreeLayoutNodes(child.Value, id, child.Key, childOrder, nodes, nodeIdsByReference, ref nextId);
            childOrder++;
        }
    }

    private static RestaurantDecisionPath BuildDecisionPath(
        DecisionTree tree,
        IExample example,
        IReadOnlyDictionary<DecisionTree, string> nodeIdsByReference)
    {
        var nodeIds = new List<string>();
        var edgeKeys = new List<string>();

        var current = tree;
        if (!nodeIdsByReference.TryGetValue(current, out var currentId))
        {
            return new RestaurantDecisionPath(nodeIds, edgeKeys, null);
        }

        nodeIds.Add(currentId);

        while (!current.IsLeaf)
        {
            var splitAttribute = current.SplitAttribute;
            if (string.IsNullOrEmpty(splitAttribute))
            {
                break;
            }

            var value = example.GetAttributeValueAsString(splitAttribute);
            if (!current.Children.TryGetValue(value, out var child))
            {
                break;
            }

            if (!nodeIdsByReference.TryGetValue(child, out var childId))
            {
                break;
            }

            edgeKeys.Add($"{currentId}->{childId}");
            nodeIds.Add(childId);
            current = child;
            currentId = childId;
        }

        return new RestaurantDecisionPath(nodeIds, edgeKeys, nodeIds.Count == 0 ? null : nodeIds[^1]);
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
