using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Learning.Framework;

public class DataSet : IDataSet
{
    public DataSet(IDataSetSpecification specification)
    {
        Examples = new List<IExample>();
        Specification = specification;
    }

    public List<IExample> Examples { get; set; }

    public IDataSetSpecification Specification { get; set; }

    public IEnumerator<IExample> GetEnumerator()
    {
        return Examples.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerable<string> GetNonTargetAttributes()
    {
        return LearningUtils.RemoveFrom(Specification.GetAttributeNames(), Specification.TargetAttribute);
    }

    public IEnumerable<string> GetPossibleAttributeValues(string attributeName)
    {
        return Specification.GetPossibleAttributeValues(attributeName);
    }

    public IDataSet EmptyDataSet()
    {
        return new DataSet(Specification);
    }

    public double CalculateGainFor(string parameterName)
    {
        var split = SplitByAttribute(parameterName);
        var totalSize = Examples.Count;
        var remainder = 0.0;
        foreach (var parameterValue in split.Keys)
        {
            var reducedDataSetSize = split[parameterValue].Examples.Count;
            var information = split[parameterValue].GetInformationFor();
            remainder += (double)reducedDataSetSize / totalSize * information;
        }

        return GetInformationFor() - remainder;
    }

    public double GetInformationFor()
    {
        var attributeName = Specification.TargetAttribute;
        var counts = new Dictionary<string, int>();
        foreach (var value in Examples.Select(example => example.GetAttributeValueAsString(attributeName)))
        {
            counts[value] = counts.TryGetValue(value, out var count) ? count + 1 : 1;
        }

        var probabilities = LearningUtils.Normalize(counts.Values.Select(Convert.ToDouble).ToList());
        return LearningUtils.Information(probabilities);
    }

    public IDataSet MatchingDataSet(string attributeName, string attributeValue)
    {
        var dataSet = new DataSet(Specification);
        foreach (var example in Examples.Where(example => example.GetAttributeValueAsString(attributeName).Equals(attributeValue)))
        {
            dataSet.Examples.Add(example);
        }

        return dataSet;
    }

    private Dictionary<string, IDataSet> SplitByAttribute(string parameterName)
    {
        var result = new Dictionary<string, IDataSet>();
        foreach (var example in Examples)
        {
            var value = example.GetAttributeValueAsString(parameterName);
            if (!result.TryGetValue(value, out var dataSet))
            {
                dataSet = new DataSet(Specification);
                result.Add(value, dataSet);
            }

            dataSet.Examples.Add(example);
        }

        return result;
    }
}
