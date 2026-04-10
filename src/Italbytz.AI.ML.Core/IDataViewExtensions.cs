using System.Collections.Immutable;
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

    public static IDataExcerpt GetDataExcerpt(this IDataView dataView, string labelColumnName = DefaultColumnNames.Label, string featureColumnName = DefaultColumnNames.Features)
    {
        var featureNames = dataView.GetFeaturesSlotNames(featureColumnName).Select(memory => memory.ToString()).ToArray();
        var features = dataView.GetColumn<float[]>(featureColumnName).ToList();
        var labels = dataView.GetColumn<uint>(labelColumnName).ToList();
        return new DataExcerpt(features, featureNames, labels);
    }
}
