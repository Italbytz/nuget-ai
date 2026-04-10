namespace Italbytz.AI.ML.Core;

public interface IDataExcerpt
{
    string[] FeatureNames { get; }

    List<uint> Labels { get; }

    List<float[]> Features { get; }

    uint[] UniqueLabelValues { get; }

    float[] GetFeatureColumn(string featureName);

    float[] GetUniqueFeatureValues(string featureName);
}
