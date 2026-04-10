using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Learning.Inductive;

namespace Italbytz.AI.Learning.Learners;

public class DecisionTreeLearner : ILearner
{
    public DecisionTreeLearner()
    {
        DefaultValue = "Unable To Classify";
    }

    public DecisionTreeLearner(DecisionTree decisionTree, string defaultValue)
    {
        Tree = decisionTree;
        DefaultValue = defaultValue;
    }

    public DecisionTree? Tree { get; set; }

    public string DefaultValue { get; set; }

    public void Train(IDataSet ds)
    {
        var attributes = ds.GetNonTargetAttributes();
        Tree = DecisionTreeLearning(ds, attributes, new ConstantDecisionTree(DefaultValue));
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

    private DecisionTree DecisionTreeLearning(IDataSet ds, IEnumerable<string> attributeNames, ConstantDecisionTree defaultTree)
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

        var chosenAttribute = ChooseAttribute(ds, attributeNames);
        var tree = new DecisionTree(chosenAttribute);
        var majority = MajorityValue(ds);
        foreach (var value in ds.GetPossibleAttributeValues(chosenAttribute))
        {
            var filtered = ds.MatchingDataSet(chosenAttribute, value);
            var newAttributes = LearningUtils.RemoveFrom(attributeNames, chosenAttribute);
            var subtree = DecisionTreeLearning(filtered, newAttributes, majority);
            tree.AddNode(value, subtree);
        }

        return tree;
    }

    private static string ChooseAttribute(IDataSet ds, IEnumerable<string> attributeNames)
    {
        var greatestGain = 0.0;
        var attributeWithGreatestGain = attributeNames.First();
        foreach (var attribute in attributeNames)
        {
            var gain = ds.CalculateGainFor(attribute);
            if (gain > greatestGain)
            {
                greatestGain = gain;
                attributeWithGreatestGain = attribute;
            }
        }

        return attributeWithGreatestGain;
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
