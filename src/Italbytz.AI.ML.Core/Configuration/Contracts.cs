using System.Collections.Generic;
using System.Text.Json.Serialization;
using Italbytz.AI.ML.Core;

namespace Italbytz.AI.ML.Core.Configuration;

[JsonDerivedType(typeof(ColumnPropertiesV5))]
public interface IColumnProperties
{
    string? ColumnName { get; set; }

    ColumnPurposeType ColumnPurpose { get; set; }

    ColumnDataKind ColumnDataFormat { get; set; }

    bool IsCategorical { get; set; }
}

[JsonDerivedType(typeof(TabularFileDataSourceV3))]
public interface IDataSource
{
    DataSourceType DataSourceType { get; set; }
}

public interface ITabularDataSource : IDataSource
{
    IEnumerable<IColumnProperties> ColumnProperties { get; set; }

    string? Delimiter { get; set; }

    char DecimalMarker { get; set; }

    bool HasHeader { get; set; }
}

public interface ITabularFileDataSource : ITabularDataSource
{
    string? FilePath { get; set; }

    bool AllowQuoting { get; set; }

    char EscapeCharacter { get; set; }

    bool ReadMultiLines { get; set; }
}

[JsonDerivedType(typeof(LocalEnvironmentV1))]
public interface IEnvironment
{
    EnvironmentType EnvironmentType { get; set; }
}

public interface IScenario
{
    ScenarioType ScenarioType { get; set; }
}

[JsonDerivedType(typeof(TrainingConfiguration))]
public interface ITrainingConfiguration
{
    ScenarioType Scenario { get; set; }

    IDataSource? DataSource { get; set; }

    IEnvironment? Environment { get; set; }

    ITrainingOption? TrainingOption { get; set; }

    AutoMLType? AutoMLType { get; set; }

    ITrainResult? TrainResult { get; set; }

    string? TrainingConfigurationFolder { get; set; }

    string SerializeToJson(bool writeIndented = false);
}

[JsonDerivedType(typeof(RegressionTrainingOptionV2))]
[JsonDerivedType(typeof(ClassificationTrainingOptionV2))]
public interface ITrainingOption
{
    int TrainingTime { get; set; }

    int? Seed { get; set; }

    string? OutputFolder { get; set; }

    IValidationOption? ValidationOption { get; set; }
}

[JsonDerivedType(typeof(FileValidationOptionV0))]
[JsonDerivedType(typeof(CrossValidationOptionV0))]
[JsonDerivedType(typeof(TrainValidationSplitOptionV0))]
public interface IValidationOption
{
}

public interface ITrainResult
{
    IEnumerable<ITrial>? Trials { get; set; }

    string? PipelineSchema { get; set; }

    Dictionary<string, EstimatorType>? Estimators { get; set; }

    string? MetricName { get; set; }

    string? ModelFilePath { get; set; }
}

public interface ITrial
{
    Parameter? Parameter { get; set; }

    string? TrainerName { get; set; }

    double Score { get; set; }

    double RuntimeInSeconds { get; set; }
}
