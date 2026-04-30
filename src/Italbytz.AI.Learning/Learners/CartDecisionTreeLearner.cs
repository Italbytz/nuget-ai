using Italbytz.AI.Learning.Inductive;

namespace Italbytz.AI.Learning.Learners;

public class CartDecisionTreeLearner : ILearner
{
    public CartDecisionTreeLearner()
    {
        DefaultValue = "Unable To Classify";
    }

    public CartDecisionTreeLearner(DecisionTree decisionTree, string defaultValue)
    {
        Tree = decisionTree;
        DefaultValue = defaultValue;
    }

    public DecisionTree? Tree { get; set; }

    public string DefaultValue { get; set; }

    public void Train(IDataSet ds)
    {
        var attributes = ds.GetNonTargetAttributes();
        Tree = BuildTree(ds, attributes, new ConstantDecisionTree(DefaultValue));
    }

    public string[] Predict(IDataSet ds)
    {
        return ds.Examples.Select(Predict).ToArray();
    }

    public string Predict(IExample e)
    {
        return (string)(Tree?.Predict(e) ?? DefaultValue);
    }

    public int[] Test(IDataSet ds)
    {
        var results = new[] { 0, 0 };
        foreach (var example in ds.Examples)
        {
            if (string.Equals(example.TargetValue(), Tree?.Predict(example)?.ToString(), StringComparison.Ordinal))
            {
                results[0]++;
            }
            else
            {
                results[1]++;
            }
        }

        return results;
    }

    private DecisionTree BuildTree(IDataSet ds, IEnumerable<string> attributeNames, ConstantDecisionTree defaultTree)
    {
        if (ds.Examples.Count == 0)
        {
            return defaultTree;
        }

        if (AllExamplesHaveSameClassification(ds))
        {
            return new ConstantDecisionTree(ds.Examples[0].TargetValue());
        }

        if (!attributeNames.Any())
        {
            return MajorityValue(ds);
        }

        var chosenAttribute = ChooseAttributeByGini(ds, attributeNames);
        var tree = new DecisionTree(chosenAttribute);
        var majority = MajorityValue(ds);

        foreach (var value in ds.GetPossibleAttributeValues(chosenAttribute))
        {
            var filtered = ds.MatchingDataSet(chosenAttribute, value);
            var newAttributes = LearningUtils.RemoveFrom(attributeNames, chosenAttribute);
            var subtree = BuildTree(filtered, newAttributes, majority);
            tree.AddNode(value, subtree);
        }

        return tree;
    }

    private static string ChooseAttributeByGini(IDataSet ds, IEnumerable<string> attributeNames)
    {
        var bestAttribute = attributeNames.First();
        var bestGini = double.PositiveInfinity;

        foreach (var attribute in attributeNames)
        {
            var weightedGini = CalculateWeightedGini(ds, attribute);
            if (weightedGini < bestGini)
            {
                bestGini = weightedGini;
                bestAttribute = attribute;
            }
        }

        return bestAttribute;
    }

    private static double CalculateWeightedGini(IDataSet ds, string attributeName)
    {
        var total = ds.Examples.Count;
        if (total == 0)
        {
            return 0;
        }

        var weighted = 0.0;
        foreach (var value in ds.GetPossibleAttributeValues(attributeName))
        {
            var split = ds.MatchingDataSet(attributeName, value);
            var splitCount = split.Examples.Count;
            if (splitCount == 0)
            {
                continue;
            }

            weighted += (double)splitCount / total * CalculateGini(split);
        }

        return weighted;
    }

    private static double CalculateGini(IDataSet ds)
    {
        var total = ds.Examples.Count;
        if (total == 0)
        {
            return 0;
        }

        var classCounts = ds.Examples
            .GroupBy(example => example.TargetValue(), StringComparer.Ordinal)
            .Select(group => group.Count());

        var sumSquared = classCounts
            .Select(count => (double)count / total)
            .Sum(probability => probability * probability);

        return 1.0 - sumSquared;
    }

    private static ConstantDecisionTree MajorityValue(IDataSet ds)
    {
        var learner = new MajorityLearner();
        learner.Train(ds);
        return new ConstantDecisionTree(learner.Predict(ds.Examples[0]));
    }

    private static bool AllExamplesHaveSameClassification(IDataSet ds)
    {
        var classification = ds.Examples[0].TargetValue();
        return ds.Examples.All(example => example.TargetValue().Equals(classification, StringComparison.Ordinal));
    }
}
