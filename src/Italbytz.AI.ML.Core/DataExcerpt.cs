namespace Italbytz.AI.ML.Core;

public class DataExcerpt(IEnumerable<float[]> features, IEnumerable<string> featureNames, IEnumerable<uint> labels) : IDataExcerpt
{
    private readonly Dictionary<string, float[]> _featureColumns = new();
    private readonly Dictionary<string, float[]> _uniqueFeatureValues = new();
    private uint[]? _uniqueLabelValues;

    public string[] FeatureNames { get; } = featureNames.ToArray();

    public List<uint> Labels { get; } = labels.ToList();

    public List<float[]> Features { get; } = features.Select(row => row.ToArray()).ToList();

    public uint[] UniqueLabelValues => _uniqueLabelValues ??= Labels.Distinct().ToArray();

    public float[] GetFeatureColumn(string featureName)
    {
        if (_featureColumns.TryGetValue(featureName, out var column))
        {
            return column;
        }

        var index = Array.IndexOf(FeatureNames, featureName);
        if (index < 0)
        {
            throw new ArgumentException($"Feature '{featureName}' not found.", nameof(featureName));
        }

        column = Features.Select(values => values[index]).ToArray();
        _featureColumns[featureName] = column;
        return column;
    }

    public float[] GetUniqueFeatureValues(string featureName)
    {
        if (_uniqueFeatureValues.TryGetValue(featureName, out var values))
        {
            return values;
        }

        values = GetFeatureColumn(featureName).Distinct().ToArray();
        _uniqueFeatureValues[featureName] = values;
        return values;
    }
}
