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
