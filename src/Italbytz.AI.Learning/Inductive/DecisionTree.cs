using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Learning.Inductive;

public class DecisionTree
{
    private readonly Dictionary<string, DecisionTree> _nodes = new();

    protected DecisionTree()
    {
    }

    public DecisionTree(string attributeName)
    {
        AttributeName = attributeName ?? throw new ArgumentNullException(nameof(attributeName));
    }

    private string? AttributeName { get; }

    public void AddLeaf(string attributeValue, string decision)
    {
        _nodes[attributeValue] = new ConstantDecisionTree(decision);
    }

    public void AddNode(string attributeValue, DecisionTree tree)
    {
        _nodes[attributeValue] = tree;
    }

    public virtual object Predict(IExample example)
    {
        var attributeValue = example.GetAttributeValueAsString(AttributeName!);
        if (_nodes.TryGetValue(attributeValue, out var node))
        {
            return node.Predict(example);
        }

        throw new InvalidOperationException($"no node exists for attribute value {attributeValue}");
    }

    public static DecisionTree GetStumpFor(IDataSet dataSet, string attributeName, string attributeValue, string returnValueIfMatched, List<string> unmatchedValues, string returnValueIfUnmatched)
    {
        var tree = new DecisionTree(attributeName);
        tree.AddLeaf(attributeValue, returnValueIfMatched);
        foreach (var unmatchedValue in unmatchedValues)
        {
            tree.AddLeaf(unmatchedValue, returnValueIfUnmatched);
        }

        return tree;
    }

    public static IEnumerable<DecisionTree> GetStumpsFor(IDataSet dataSet, string returnValueIfMatched, string returnValueIfUnmatched)
    {
        return (from attribute in dataSet.GetNonTargetAttributes()
                from value in dataSet.GetPossibleAttributeValues(attribute)
                let unmatchedValues = LearningUtils.RemoveFrom(dataSet.GetPossibleAttributeValues(attribute), value)
                select GetStumpFor(dataSet, attribute, value, returnValueIfMatched, unmatchedValues, returnValueIfUnmatched)).ToList();
    }
}
