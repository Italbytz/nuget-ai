using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class SolarFlareDataset : Dataset<SolarFlareDataset.SolarFlareModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    public override string FilePrefix => "solar_flare";

    public override string? LabelColumnName => "flares";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "modified Zurich class", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "largest spot size", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "spot distribution", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "activity", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "evolution", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "previous 24 hour flare activity", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "historically-complex", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "became complex on this pass", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "area", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "area of largest spot", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "flares", ColumnPurpose = ColumnPurposeType.Label, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true }
    ];

    public override IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFileInternal(path, separatorChar, hasHeader, allowQuoting, trimWhitespace, allowSparse);
    }

    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.ReplaceMissingValues(new[]
        {
            new InputOutputColumnPair("activity"),
            new InputOutputColumnPair("evolution"),
            new InputOutputColumnPair("previous 24 hour flare activity"),
            new InputOutputColumnPair("historically-complex"),
            new InputOutputColumnPair("became complex on this pass"),
            new InputOutputColumnPair("area"),
            new InputOutputColumnPair("area of largest spot")
        });
    }

    protected override IEstimator<ITransformer>? BuildLabelMappingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Conversion
            .MapValueToKey("flares", "flares", addKeyValueAnnotationsAsText: false)
            .Append(mlContext.Transforms.CopyColumns(DefaultColumnNames.Label, "flares"));
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Categorical.OneHotEncoding(new[]
            {
                new InputOutputColumnPair("modified Zurich class", "modified Zurich class"),
                new InputOutputColumnPair("largest spot size", "largest spot size"),
                new InputOutputColumnPair("spot distribution", "spot distribution")
            })
            .Append(mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
                "modified Zurich class",
                "largest spot size",
                "spot distribution",
                "activity",
                "evolution",
                "previous 24 hour flare activity",
                "historically-complex",
                "became complex on this pass",
                "area",
                "area of largest spot"));
    }

    public class SolarFlareModelInput
    {
        [LoadColumn(0)] [ColumnName("modified Zurich class")] public string ModifiedZurichClass { get; set; } = string.Empty;
        [LoadColumn(1)] [ColumnName("largest spot size")] public string LargestSpotSize { get; set; } = string.Empty;
        [LoadColumn(2)] [ColumnName("spot distribution")] public string SpotDistribution { get; set; } = string.Empty;
        [LoadColumn(3)] [ColumnName("activity")] public float Activity { get; set; }
        [LoadColumn(4)] [ColumnName("evolution")] public float Evolution { get; set; }
        [LoadColumn(5)] [ColumnName("previous 24 hour flare activity")] public float Previous24HourFlareActivity { get; set; }
        [LoadColumn(6)] [ColumnName("historically-complex")] public float HistoricallyComplex { get; set; }
        [LoadColumn(7)] [ColumnName("became complex on this pass")] public float BecameComplexOnThisPass { get; set; }
        [LoadColumn(8)] [ColumnName("area")] public float Area { get; set; }
        [LoadColumn(9)] [ColumnName("area of largest spot")] public float AreaOfLargestSpot { get; set; }
        [LoadColumn(10)] [ColumnName("flares")] public float Flares { get; set; }
    }
}
