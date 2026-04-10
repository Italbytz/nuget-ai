using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class WineDataset : Dataset<WineDataset.WineModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    public override string FilePrefix => "wine";

    public override string? LabelColumnName => "class";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "Alcohol", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Malicacid", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Ash", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Alcalinity_of_ash", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Magnesium", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Total_phenols", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Flavanoids", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Nonflavanoid_phenols", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Proanthocyanins", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Color_intensity", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Hue", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "0D280_0D315_of_diluted_wines", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Proline", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "class", ColumnPurpose = ColumnPurposeType.Label, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true }
    ];

    public override IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFileInternal(path, separatorChar, hasHeader, allowQuoting, trimWhitespace, allowSparse);
    }

    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.ReplaceMissingValues(new[]
        {
            new InputOutputColumnPair("Alcohol"),
            new InputOutputColumnPair("Malicacid"),
            new InputOutputColumnPair("Ash"),
            new InputOutputColumnPair("Alcalinity_of_ash"),
            new InputOutputColumnPair("Magnesium"),
            new InputOutputColumnPair("Total_phenols"),
            new InputOutputColumnPair("Flavanoids"),
            new InputOutputColumnPair("Nonflavanoid_phenols"),
            new InputOutputColumnPair("Proanthocyanins"),
            new InputOutputColumnPair("Color_intensity"),
            new InputOutputColumnPair("Hue"),
            new InputOutputColumnPair("0D280_0D315_of_diluted_wines"),
            new InputOutputColumnPair("Proline")
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
            "Alcohol", "Malicacid", "Ash", "Alcalinity_of_ash", "Magnesium", "Total_phenols", "Flavanoids", "Nonflavanoid_phenols", "Proanthocyanins", "Color_intensity", "Hue", "0D280_0D315_of_diluted_wines", "Proline");
    }

    public class WineModelInput
    {
        [LoadColumn(0)] [ColumnName("Alcohol")] public float Alcohol { get; set; }
        [LoadColumn(1)] [ColumnName("Malicacid")] public float Malicacid { get; set; }
        [LoadColumn(2)] [ColumnName("Ash")] public float Ash { get; set; }
        [LoadColumn(3)] [ColumnName("Alcalinity_of_ash")] public float AlcalinityOfAsh { get; set; }
        [LoadColumn(4)] [ColumnName("Magnesium")] public float Magnesium { get; set; }
        [LoadColumn(5)] [ColumnName("Total_phenols")] public float TotalPhenols { get; set; }
        [LoadColumn(6)] [ColumnName("Flavanoids")] public float Flavanoids { get; set; }
        [LoadColumn(7)] [ColumnName("Nonflavanoid_phenols")] public float NonflavanoidPhenols { get; set; }
        [LoadColumn(8)] [ColumnName("Proanthocyanins")] public float Proanthocyanins { get; set; }
        [LoadColumn(9)] [ColumnName("Color_intensity")] public float ColorIntensity { get; set; }
        [LoadColumn(10)] [ColumnName("Hue")] public float Hue { get; set; }
        [LoadColumn(11)] [ColumnName("0D280_0D315_of_diluted_wines")] public float _0D280_0D315OfDilutedWines { get; set; }
        [LoadColumn(12)] [ColumnName("Proline")] public float Proline { get; set; }
        [LoadColumn(13)] [ColumnName("class")] public float Class { get; set; }
    }
}
