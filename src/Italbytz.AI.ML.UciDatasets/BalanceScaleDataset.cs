using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class BalanceScaleDataset : Dataset<BalanceScaleDataset.BalanceScaleModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    public override string FilePrefix => "balance_scale";

    public override string? LabelColumnName => "class";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "right-distance", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "right-weight", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "left-distance", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "left-weight", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "class", ColumnPurpose = ColumnPurposeType.Label, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true }
    ];

    public override IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFileInternal(path, separatorChar, hasHeader, allowQuoting, trimWhitespace, allowSparse);
    }

    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.ReplaceMissingValues(new[]
        {
            new InputOutputColumnPair("right-distance"),
            new InputOutputColumnPair("right-weight"),
            new InputOutputColumnPair("left-distance"),
            new InputOutputColumnPair("left-weight")
        });
    }

    protected override IEstimator<ITransformer>? BuildLabelMappingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Conversion
            .MapValueToKey("class", "class", addKeyValueAnnotationsAsText: false)
            .Append(mlContext.Transforms.CopyColumns(DefaultColumnNames.Label, "class"));
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
            "right-distance", "right-weight", "left-distance", "left-weight");
    }

    public class BalanceScaleModelInput
    {
        [LoadColumn(0)] [ColumnName("right-distance")] public float RightDistance { get; set; }
        [LoadColumn(1)] [ColumnName("right-weight")] public float RightWeight { get; set; }
        [LoadColumn(2)] [ColumnName("left-distance")] public float LeftDistance { get; set; }
        [LoadColumn(3)] [ColumnName("left-weight")] public float LeftWeight { get; set; }
        [LoadColumn(4)] [ColumnName("class")] public string Class { get; set; } = string.Empty;
    }
}
