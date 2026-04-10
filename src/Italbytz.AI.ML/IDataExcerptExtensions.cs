using System.Globalization;
using Italbytz.AI.Learning;
using Italbytz.AI.Learning.Framework;
using Italbytz.AI.ML.Core;

namespace Italbytz.AI.ML;

public static class IDataExcerptExtensions
{
    public static IDataSetSpecification GetDataSetSpecification(this IDataExcerpt dataExcerpt)
    {
        var specification = new DataSetSpecification();
        foreach (var featureName in dataExcerpt.FeatureNames)
        {
            specification.DefineStringAttribute(
                featureName,
                dataExcerpt.GetUniqueFeatureValues(featureName)
                    .Select(value => value.ToString(CultureInfo.InvariantCulture))
                    .ToArray());
        }

        specification.DefineStringAttribute(
            DefaultColumnNames.Label,
            dataExcerpt.UniqueLabelValues
                .Select(value => value.ToString(CultureInfo.InvariantCulture))
                .ToArray());

        return specification;
    }

    public static IDataSet GetDataSet(this IDataExcerpt dataExcerpt, IDataSetSpecification specification)
    {
        var dataSet = new DataSet(specification);
        for (var rowIndex = 0; rowIndex < dataExcerpt.Features.Count; rowIndex++)
        {
            Dictionary<string, IAttribute> attributes = new();
            for (var columnIndex = 0; columnIndex < dataExcerpt.FeatureNames.Length; columnIndex++)
            {
                var featureName = dataExcerpt.FeatureNames[columnIndex];
                var featureValue = dataExcerpt.Features[rowIndex][columnIndex].ToString(CultureInfo.InvariantCulture);
                attributes[featureName] = new StringAttribute(featureValue, specification.GetAttributeSpecFor(featureName));
            }

            var targetAttribute = new StringAttribute(
                dataExcerpt.Labels[rowIndex].ToString(CultureInfo.InvariantCulture),
                specification.GetAttributeSpecFor(DefaultColumnNames.Label));
            attributes[DefaultColumnNames.Label] = targetAttribute;
            dataSet.Examples.Add(new Example(attributes, targetAttribute));
        }

        return dataSet;
    }
}
