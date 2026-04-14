using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.Core.Configuration;
using Microsoft.ML;

namespace Italbytz.AI.ML.UciDatasets;

public interface IDataset
{
    string? LabelColumnName { get; }

    IDataView DataView { get; }

    ColumnPropertiesV5[] ColumnProperties { get; }

    string FilePrefix { get; }

    IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null);

    IEnumerable<TrainValidateTestFileNames> GetTrainValidateTestFiles(
        string saveFolderPath,
        string? samplingKeyColumnName = null,
        double validateFraction = 0.15,
        double testFraction = 0.15,
        int[]? seeds = null);

    IEstimator<ITransformer> BuildPreprocessingPipeline(MLContext mlContext);

    IEstimator<ITransformer> BuildPreprocessingPipeline(MLContext mlContext, ScenarioType scenarioType, ProcessingType processingType);

    IEstimator<ITransformer> BuildPipeline(MLContext mlContext, IEstimator<ITransformer> estimator);

    IEstimator<ITransformer> BuildPipeline(MLContext mlContext, IEstimator<ITransformer> estimator, ScenarioType scenarioType, ProcessingType processingType);
}
