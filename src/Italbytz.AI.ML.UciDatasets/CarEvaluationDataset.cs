using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class CarEvaluationDataset : Dataset<CarEvaluationDataset.CarEvaluationModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    public override string FilePrefix => "car_evaluation";

    public override string? LabelColumnName => "class";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "buying", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "maint", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "doors", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "persons", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "lug_boot", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "safety", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "class", ColumnPurpose = ColumnPurposeType.Label, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true }
    ];

    public override IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFileInternal(path, separatorChar, hasHeader, allowQuoting, trimWhitespace, allowSparse);
    }

    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.ReplaceMissingValues([]);
    }

    protected override IEstimator<ITransformer>? BuildLabelMappingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Conversion
            .MapValueToKey("class", "class", addKeyValueAnnotationsAsText: false)
            .Append(mlContext.Transforms.CopyColumns(DefaultColumnNames.Label, "class"));
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Categorical.OneHotEncoding(new[]
            {
                new InputOutputColumnPair("buying", "buying"),
                new InputOutputColumnPair("maint", "maint"),
                new InputOutputColumnPair("doors", "doors"),
                new InputOutputColumnPair("persons", "persons"),
                new InputOutputColumnPair("lug_boot", "lug_boot"),
                new InputOutputColumnPair("safety", "safety")
            })
            .Append(mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
                "buying", "maint", "doors", "persons", "lug_boot", "safety"));
    }

    public class CarEvaluationModelInput
    {
        [LoadColumn(0)] [ColumnName("buying")] public string Buying { get; set; } = string.Empty;
        [LoadColumn(1)] [ColumnName("maint")] public string Maint { get; set; } = string.Empty;
        [LoadColumn(2)] [ColumnName("doors")] public string Doors { get; set; } = string.Empty;
        [LoadColumn(3)] [ColumnName("persons")] public string Persons { get; set; } = string.Empty;
        [LoadColumn(4)] [ColumnName("lug_boot")] public string LugBoot { get; set; } = string.Empty;
        [LoadColumn(5)] [ColumnName("safety")] public string Safety { get; set; } = string.Empty;
        [LoadColumn(6)] [ColumnName("class")] public string Class { get; set; } = string.Empty;
    }
}
