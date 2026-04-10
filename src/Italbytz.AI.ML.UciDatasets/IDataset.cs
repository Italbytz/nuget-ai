using Italbytz.AI.ML.Core;
using Microsoft.ML;

namespace Italbytz.AI.ML.UciDatasets;

public interface IDataset
{
    string? LabelColumnName { get; }

    ColumnPropertiesV5[] ColumnProperties { get; }

    string FilePrefix { get; }

    IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null);

    IEstimator<ITransformer> BuildPreprocessingPipeline(MLContext mlContext);

    IEstimator<ITransformer> BuildPipeline(MLContext mlContext, IEstimator<ITransformer> estimator);
}
