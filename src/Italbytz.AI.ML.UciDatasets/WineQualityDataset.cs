using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class WineQualityDataset : Dataset<WineQualityDataset.WineQualityModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    public override string FilePrefix => "wine_quality";

    public override string? LabelColumnName => "quality";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "fixed_acidity", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "volatile_acidity", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "citric_acid", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "residual_sugar", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "chlorides", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "free_sulfur_dioxide", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "total_sulfur_dioxide", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "density", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "pH", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "sulphates", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "alcohol", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "quality", ColumnPurpose = ColumnPurposeType.Label, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false }
    ];

    public override IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFileInternal(path, separatorChar, hasHeader, allowQuoting, trimWhitespace, allowSparse);
    }

    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.ReplaceMissingValues(new[]
        {
            new InputOutputColumnPair("fixed_acidity"),
            new InputOutputColumnPair("volatile_acidity"),
            new InputOutputColumnPair("citric_acid"),
            new InputOutputColumnPair("residual_sugar"),
            new InputOutputColumnPair("chlorides"),
            new InputOutputColumnPair("free_sulfur_dioxide"),
            new InputOutputColumnPair("total_sulfur_dioxide"),
            new InputOutputColumnPair("density"),
            new InputOutputColumnPair("pH"),
            new InputOutputColumnPair("sulphates"),
            new InputOutputColumnPair("alcohol")
        });
    }

    protected override IEstimator<ITransformer>? BuildLabelMappingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Conversion
            .MapValueToKey("quality", "quality", addKeyValueAnnotationsAsText: false)
            .Append(mlContext.Transforms.CopyColumns(DefaultColumnNames.Label, "quality"));
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
            "fixed_acidity", "volatile_acidity", "citric_acid", "residual_sugar", "chlorides", "free_sulfur_dioxide", "total_sulfur_dioxide", "density", "pH", "sulphates", "alcohol");
    }

    public class WineQualityModelInput
    {
        [LoadColumn(0)] [ColumnName("fixed_acidity")] public float FixedAcidity { get; set; }
        [LoadColumn(1)] [ColumnName("volatile_acidity")] public float VolatileAcidity { get; set; }
        [LoadColumn(2)] [ColumnName("citric_acid")] public float CitricAcid { get; set; }
        [LoadColumn(3)] [ColumnName("residual_sugar")] public float ResidualSugar { get; set; }
        [LoadColumn(4)] [ColumnName("chlorides")] public float Chlorides { get; set; }
        [LoadColumn(5)] [ColumnName("free_sulfur_dioxide")] public float FreeSulfurDioxide { get; set; }
        [LoadColumn(6)] [ColumnName("total_sulfur_dioxide")] public float TotalSulfurDioxide { get; set; }
        [LoadColumn(7)] [ColumnName("density")] public float Density { get; set; }
        [LoadColumn(8)] [ColumnName("pH")] public float PH { get; set; }
        [LoadColumn(9)] [ColumnName("sulphates")] public float Sulphates { get; set; }
        [LoadColumn(10)] [ColumnName("alcohol")] public float Alcohol { get; set; }
        [LoadColumn(11)] [ColumnName("quality")] public float Quality { get; set; }
    }
}
