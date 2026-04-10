using System.Collections.Immutable;
using System.Reflection;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.Core;

public static class IDataViewExtensions
{
    public static ImmutableArray<ReadOnlyMemory<char>> GetFeaturesSlotNames(this IDataView dataView, string columnName = DefaultColumnNames.Features)
    {
        var featuresColumn = dataView.Schema.GetColumnOrNull(columnName);
        if (featuresColumn == null)
        {
            throw new ArgumentException($"The data view does not contain a column named '{columnName}'.", nameof(columnName));
        }

        VBuffer<ReadOnlyMemory<char>> slotNames = default;
        if (featuresColumn.Value.Annotations.Schema.GetColumnOrNull("SlotNames") != null)
        {
            featuresColumn.Value.Annotations.GetValue("SlotNames", ref slotNames);
            var values = slotNames.GetValues();
            if (values.Length > 0)
            {
                return [.. values];
            }
        }

        if (featuresColumn.Value.Type is VectorDataViewType vectorType && vectorType.Size > 0)
        {
            return [.. Enumerable.Range(0, vectorType.Size).Select(index => (ReadOnlyMemory<char>)$"F{index}".AsMemory())];
        }

        return [];
    }

    public static List<IFeature> GetFeatures<ModelInput>(this IDataView dataView, string labelColumnName = DefaultColumnNames.Label)
        where ModelInput : class, new()
    {
        List<IFeature> result = [];
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        foreach (var propertyInfo in typeof(ModelInput).GetProperties(flags))
        {
            var propertyType = propertyInfo.PropertyType;
            var propertyName = propertyInfo.Name;
            var columnName = propertyInfo.Name;
            var columnIndex = 0;

            foreach (var customAttribute in propertyInfo.GetCustomAttributes(true))
            {
                switch (customAttribute)
                {
                    case ColumnNameAttribute columnNameAttribute:
                    {
                        var namePropertyInfo = columnNameAttribute.GetType().GetProperty("Name", flags);
                        if (namePropertyInfo?.GetValue(columnNameAttribute) is string configuredColumnName)
                        {
                            columnName = configuredColumnName;
                        }

                        break;
                    }
                    case LoadColumnAttribute loadColumnAttribute:
                    {
                        var sourcesFieldInfo = loadColumnAttribute.GetType().GetField("Sources", flags);
                        if (sourcesFieldInfo?.GetValue(loadColumnAttribute) is List<TextLoader.Range> ranges)
                        {
                            columnIndex = ranges.First().Min;
                        }

                        break;
                    }
                }
            }

            if (string.Equals(propertyName, labelColumnName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(columnName, labelColumnName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            IFeature? feature = null;
            if (propertyType == typeof(float))
            {
                feature = new NumericalFeature
                {
                    PropertyName = propertyName,
                    ColumnName = columnName,
                    ColumnIndex = columnIndex
                };
            }
            else if (propertyType == typeof(string))
            {
                feature = new CategoricalFeature
                {
                    PropertyName = propertyName,
                    ColumnName = columnName,
                    ColumnIndex = columnIndex
                };
            }

            if (feature != null)
            {
                result.Add(feature);
            }
        }

        foreach (var feature in result)
        {
            switch (feature)
            {
                case NumericalFeature numericalFeature:
                    numericalFeature.ValueRange = [
                        dataView.GetColumn<float>(feature.ColumnName).Min(),
                        dataView.GetColumn<float>(feature.ColumnName).Max()
                    ];
                    break;
                case CategoricalFeature categoricalFeature:
                    categoricalFeature.ValueRange = [.. dataView.GetColumn<string>(feature.ColumnName).Distinct().OrderBy(value => value)];
                    break;
            }
        }

        return result;
    }

    public static IDataExcerpt GetDataExcerpt(this IDataView dataView, string labelColumnName = DefaultColumnNames.Label, string featureColumnName = DefaultColumnNames.Features)
    {
        var featureNames = dataView.GetFeaturesSlotNames(featureColumnName).Select(memory => memory.ToString()).ToArray();
        var features = dataView.GetColumn<float[]>(featureColumnName).ToList();
        var labels = dataView.GetColumn<uint>(labelColumnName).ToList();
        return new DataExcerpt(features, featureNames, labels);
    }
}
