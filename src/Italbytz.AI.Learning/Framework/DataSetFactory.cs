using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Learning.Framework;

public static class DataSetFactory
{
    public static IDataSet FromString(string data, DataSetSpecification specification, string separator)
    {
        var dataSet = new DataSet(specification);
        var lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            dataSet.Examples.Add(ExampleFromString(line, specification, separator));
        }

        return dataSet;
    }

    private static IExample ExampleFromString(string data, IDataSetSpecification dataSetSpecification, string separator)
    {
        var attributes = new Dictionary<string, IAttribute>();
        var attributeValues = data.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (!dataSetSpecification.IsValid(attributeValues))
        {
            throw new InvalidOperationException($"Unable to construct Example from {data}");
        }

        foreach (var pair in dataSetSpecification.GetAttributeNames().Zip(attributeValues, Tuple.Create))
        {
            var attributeSpecification = dataSetSpecification.GetAttributeSpecFor(pair.Item1);
            attributes.Add(pair.Item1, attributeSpecification.CreateAttribute(pair.Item2));
        }

        var targetAttributeName = dataSetSpecification.TargetAttribute;
        return new Example(attributes, attributes[targetAttributeName]);
    }
}
