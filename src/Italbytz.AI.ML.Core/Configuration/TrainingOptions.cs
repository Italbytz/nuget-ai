namespace Italbytz.AI.ML.Core.Configuration;

public class ClassificationTrainingOptionV2 : MBConfig, ITrainingOption
{
    public override int Version => 2;

    public override string? Type { get; set; } = "ClassificationTrainingOption";

    public bool Subsampling { get; set; }

    public string? LabelColumn { get; set; }

    public string[]? AvailableTrainers { get; set; }

    public int TrainingTime { get; set; }

    public int? Seed { get; set; }

    public string? OutputFolder { get; set; }

    public IValidationOption? ValidationOption { get; set; }
}

public class RegressionTrainingOptionV2 : MBConfig, ITrainingOption
{
    public override int Version => 2;

    public override string? Type { get; set; } = "RegressionTrainingOption";

    public int? MaxModelToExplore { get; set; }

    public int? MaximumMemoryToUseInMB { get; set; }

    public bool Subsampling { get; set; }

    public string? LabelColumn { get; set; }

    public string[]? AvailableTrainers { get; set; }

    public string? Tuner { get; set; }

    public string? OptimizeMetric { get; set; }

    public int TrainingTime { get; set; }

    public int? Seed { get; set; }

    public string? OutputFolder { get; set; }

    public IValidationOption? ValidationOption { get; set; }
}

public class TrainValidationSplitOptionV0 : MBConfig, IValidationOption
{
    public override int Version => 0;

    public override string? Type { get; set; } = "TrainValidateSplitValidationOption";

    public float? SplitRatio { get; set; }
}

public class CrossValidationOptionV0 : MBConfig, IValidationOption
{
    public override int Version => 0;

    public override string? Type { get; set; } = "CrossValidationValidationOption";

    public int? NumberOfFolds { get; set; }
}

public class FileValidationOptionV0 : MBConfig, IValidationOption
{
    public override int Version => 0;

    public override string? Type { get; set; } = "FileValidationOption";

    public string? FilePath { get; set; }
}

public class DefaultTrainingOptionV1 : MBConfig, ITrainingOption
{
    public override int Version => 1;

    public override string? Type { get; set; } = "DefaultTrainingOption";

    public int TrainingTime { get; set; }

    public int? Seed { get; set; }

    public string? OutputFolder { get; set; }

    public IValidationOption? ValidationOption { get; set; }
}

public class ForecastingTrainingOptionV3 : MBConfig, ITrainingOption
{
    public override int Version => 3;

    public override string? Type { get; set; } = "ForecastingTrainingOption";

    public int? Horizon { get; set; }

    public string? TimeColumn { get; set; }

    public string? LabelColumn { get; set; }

    public bool UseDefaultIndex { get; set; }

    public string? OptimizeMetric { get; set; }

    public int? MaxModel { get; set; }

    public long? MaximumMemoryUsageInMegaByte { get; set; }

    public string? Tuner { get; set; }

    public int TrainingTime { get; set; }

    public int? Seed { get; set; }

    public string? OutputFolder { get; set; }

    public IValidationOption? ValidationOption { get; set; }
}

public class RecommendationTrainingOptionV2 : MBConfig, ITrainingOption
{
    public override int Version => 2;

    public override string? Type { get; set; } = "RecommendationTrainingOption";

    public string? LabelColumn { get; set; }

    public string? UserIdColumn { get; set; }

    public string? ItemIdColumn { get; set; }

    public string[]? AvailableTrainers { get; set; }

    public string? OptimizeMetric { get; set; }

    public int? MaxModelToExplore { get; set; }

    public int? MaximumMemoryToUseInMB { get; set; }

    public bool Subsampling { get; set; }

    public string? Tuner { get; set; }

    public int TrainingTime { get; set; }

    public int? Seed { get; set; }

    public string? OutputFolder { get; set; }

    public IValidationOption? ValidationOption { get; set; }
}

public class TextClassificationTrainingOptionV1 : MBConfig, ITrainingOption
{
    public override int Version => 1;

    public override string? Type { get; set; } = "TextClassificationTrainingOption";

    public string? TextColumn { get; set; }

    public string? LabelColumn { get; set; }

    public string? OptimizeMetric { get; set; }

    public int? Epoch { get; set; }

    public int? BatchSize { get; set; }

    public int TrainingTime { get; set; }

    public int? Seed { get; set; }

    public string? OutputFolder { get; set; }

    public IValidationOption? ValidationOption { get; set; }
}

public class SentenceSimilarityTrainingOptionV1 : MBConfig, ITrainingOption
{
    public override int Version => 1;

    public override string? Type { get; set; } = "SentenceSimilarityTrainingOption";

    public string? SourceSentence { get; set; }

    public string? ComparisonSentence { get; set; }

    public string? LabelColumn { get; set; }

    public string? OptimizeMetric { get; set; }

    public int? Epoch { get; set; }

    public int? BatchSize { get; set; }

    public int TrainingTime { get; set; }

    public int? Seed { get; set; }

    public string? OutputFolder { get; set; }

    public IValidationOption? ValidationOption { get; set; }
}

public class NERTrainingOptionV0 : MBConfig, ITrainingOption
{
    public override int Version => 0;

    public override string? Type { get; set; } = "NERTrainingOption";

    public string? SentenceColumn { get; set; }

    public string? LabelColumn { get; set; }

    public string? LabelsFilePath { get; set; }

    public string? OptimizeMetric { get; set; }

    public int? Epoch { get; set; }

    public int? BatchSize { get; set; }

    public int TrainingTime { get; set; }

    public int? Seed { get; set; }

    public string? OutputFolder { get; set; }

    public IValidationOption? ValidationOption { get; set; }
}

public class QuestionAnswerTrainingOptionV0 : MBConfig, ITrainingOption
{
    public override int Version => 0;

    public override string? Type { get; set; } = "QuestionAnswerTrainingOption";

    public string? ContextColumn { get; set; }

    public string? QuestionColumn { get; set; }

    public string? AnswerIndexColumn { get; set; }

    public string? LabelColumn { get; set; }

    public string? OptimizeMetric { get; set; }

    public int? Epoch { get; set; }

    public int? TopKAnswer { get; set; }

    public int? BatchSize { get; set; }

    public int TrainingTime { get; set; }

    public int? Seed { get; set; }

    public string? OutputFolder { get; set; }

    public IValidationOption? ValidationOption { get; set; }
}

public class LocalObjectDetectionTrainingOptionV0 : MBConfig, ITrainingOption
{
    public override int Version => 0;

    public override string? Type { get; set; } = "LocalObjectDetectionTrainingOption";

    public string? OptimizeMetric { get; set; }

    public int? BatchSize { get; set; }

    public int? Epoch { get; set; }

    public float? ScoreThreshold { get; set; }

    public float? IOUThreshold { get; set; }

    public int TrainingTime { get; set; }

    public int? Seed { get; set; }

    public string? OutputFolder { get; set; }

    public IValidationOption? ValidationOption { get; set; }
}

public class AzureObjectDetectionTrainingOptionV0 : MBConfig, ITrainingOption
{
    public override int Version => 0;

    public override string? Type { get; set; } = "AzureObjectDetectionTrainingOption";

    public string? OptimizeMetric { get; set; }

    public int TrainingTime { get; set; }

    public int? Seed { get; set; }

    public string? OutputFolder { get; set; }

    public IValidationOption? ValidationOption { get; set; }
}
