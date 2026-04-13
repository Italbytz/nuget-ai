using Microsoft.ML;

namespace Italbytz.ML.Data;

public enum ProcessingType
{
    Standard,
    FeatureBinningAndCustomLabelMapping
}

public record TrainValidateTestFileNames
{
    public required string TrainFileName { get; set; }
    public required string ValidateFileName { get; set; }
    public required string TrainValidateFileName { get; set; }
    public required string TestFileName { get; set; }
}

public interface IDataset
{
    string? LabelColumnName { get; }
    IDataView DataView { get; }
    Italbytz.AI.ML.Core.ColumnPropertiesV5[] ColumnProperties { get; }
    string FilePrefix { get; }

    IEnumerable<TrainValidateTestFileNames> GetTrainValidateTestFiles(
        string saveFolderPath,
        string? samplingKeyColumnName = null,
        double validateFraction = 0.15,
        double testFraction = 0.15,
        int[]? seeds = null);

    IEstimator<ITransformer> BuildPipeline(
        MLContext mlContext,
        IEstimator<ITransformer> estimator,
        Italbytz.ML.ModelBuilder.Configuration.ScenarioType scenarioType = Italbytz.ML.ModelBuilder.Configuration.ScenarioType.Classification,
        ProcessingType processingType = ProcessingType.Standard);

    IDataView LoadFromTextFile(
        string path,
        char? separatorChar = null,
        bool? hasHeader = null,
        bool? allowQuoting = null,
        bool? trimWhitespace = null,
        bool? allowSparse = null);
}

internal sealed class LegacyDatasetAdapter : IDataset
{
    private readonly Italbytz.AI.ML.UciDatasets.IDataset _inner;

    public LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.IDataset inner)
    {
        _inner = inner;
    }

    public string? LabelColumnName => _inner.LabelColumnName;

    public IDataView DataView => ((dynamic)_inner).DataView;

    public Italbytz.AI.ML.Core.ColumnPropertiesV5[] ColumnProperties => _inner.ColumnProperties;

    public string FilePrefix => _inner.FilePrefix;

    public IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null)
        => _inner.LoadFromTextFile(path, separatorChar, hasHeader, allowQuoting, trimWhitespace, allowSparse);

    public IEstimator<ITransformer> BuildPipeline(MLContext mlContext, IEstimator<ITransformer> estimator, Italbytz.ML.ModelBuilder.Configuration.ScenarioType scenarioType = Italbytz.ML.ModelBuilder.Configuration.ScenarioType.Classification, ProcessingType processingType = ProcessingType.Standard)
    {
        var mappedScenario = scenarioType switch
        {
            Italbytz.ML.ModelBuilder.Configuration.ScenarioType.Classification => Italbytz.AI.ML.Core.Configuration.ScenarioType.Classification,
            Italbytz.ML.ModelBuilder.Configuration.ScenarioType.Regression => Italbytz.AI.ML.Core.Configuration.ScenarioType.Regression,
            _ => Italbytz.AI.ML.Core.Configuration.ScenarioType.Classification
        };

        var mappedProcessing = processingType switch
        {
            ProcessingType.FeatureBinningAndCustomLabelMapping => Italbytz.AI.ML.UciDatasets.ProcessingType.FeatureBinningAndCustomLabelMapping,
            _ => Italbytz.AI.ML.UciDatasets.ProcessingType.Standard
        };

        return ((dynamic)_inner).BuildPipeline(mlContext, estimator, mappedScenario, mappedProcessing);
    }

    public IEnumerable<TrainValidateTestFileNames> GetTrainValidateTestFiles(string saveFolderPath, string? samplingKeyColumnName = null, double validateFraction = 0.15, double testFraction = 0.15, int[]? seeds = null)
    {
        var mlContext = new MLContext();
        var generatedFiles = new List<TrainValidateTestFileNames>();
        seeds ??= [int.MinValue];

        foreach (var seed in seeds)
        {
            int? realSeed = seed == int.MinValue ? null : seed;
            var trainTestSplit = mlContext.Data.TrainTestSplit(DataView, testFraction, samplingKeyColumnName, realSeed);
            var trainValidateDataSet = trainTestSplit.TrainSet;
            var testDataSet = trainTestSplit.TestSet;
            var validateInTrainFraction = validateFraction / (1 - testFraction);
            var validateTrainSplit = mlContext.Data.TrainTestSplit(trainValidateDataSet, validateInTrainFraction, samplingKeyColumnName, realSeed);

            var validateDataSet = validateTrainSplit.TestSet;
            var trainDataSet = validateTrainSplit.TrainSet;

            var seedString = seed == int.MinValue ? string.Empty : "_seed" + seed;
            var files = new TrainValidateTestFileNames
            {
                TrainFileName = FilePrefix + "_train" + seedString + ".csv",
                ValidateFileName = FilePrefix + "_validate" + seedString + ".csv",
                TrainValidateFileName = FilePrefix + "_train_validate" + seedString + ".csv",
                TestFileName = FilePrefix + "_test" + seedString + ".csv"
            };

            SaveAsCsv(trainDataSet, Path.Combine(saveFolderPath, files.TrainFileName));
            SaveAsCsv(validateDataSet, Path.Combine(saveFolderPath, files.ValidateFileName));
            SaveAsCsv(trainValidateDataSet, Path.Combine(saveFolderPath, files.TrainValidateFileName));
            SaveAsCsv(testDataSet, Path.Combine(saveFolderPath, files.TestFileName));

            generatedFiles.Add(files);
        }

        return generatedFiles;
    }

    private static void SaveAsCsv(IDataView dataView, string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);
        new MLContext().Data.SaveAsText(dataView, stream, ',', schema: false);
    }
}

public static class Data
{
    public static IDataset AdultIncome { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.AdultIncome);
    public static IDataset Automobile { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.Automobile);
    public static IDataset Iris { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.Iris);
    public static IDataset HeartDisease { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.HeartDisease);
    public static IDataset HeartDiseaseBinary { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.HeartDiseaseBinary);
    public static IDataset WineQuality { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.WineQuality);
    public static IDataset BreastCancerWisconsinDiagnostic { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.BreastCancerWisconsinDiagnostic);
    public static IDataset CarEvaluation { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.CarEvaluation);
    public static IDataset SolarFlare1 { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.SolarFlare);
    public static IDataset NPHA { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.NPHA);
    public static IDataset CDCDiabetes { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.CDCDiabetes);
    public static IDataset ObesityLevels { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.ObesityLevels);
    public static IDataset Wine { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.Wine);
    public static IDataset Lenses { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.Lenses);
    public static IDataset BalanceScale { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.BalanceScale);
    public static IDataset PageBlocks { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.PageBlocks);
    public static IDataset BanknoteAuthentication { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.BanknoteAuthentication);
    public static IDataset StudentPerformance { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.StudentPerformance);
    public static IDataset Multiplexer6 { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.Multiplexer6);
    public static IDataset Multiplexer11 { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.Multiplexer11);
    public static IDataset Multiplexer20 { get; } = new LegacyDatasetAdapter(Italbytz.AI.ML.UciDatasets.Data.Multiplexer20);
}
