using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class LensesDataset : Dataset<LensesDataset.LensesModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    public override string FilePrefix => "lenses";

    public override string? LabelColumnName => "class";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "age", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "spectacle_prescription", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "astigmatic", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
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
            new InputOutputColumnPair("age"),
            new InputOutputColumnPair("spectacle_prescription"),
            new InputOutputColumnPair("astigmatic")
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
            "age", "spectacle_prescription", "astigmatic");
    }

    public class LensesModelInput
    {
        [LoadColumn(0)] [ColumnName("age")] public float Age { get; set; }
        [LoadColumn(1)] [ColumnName("spectacle_prescription")] public float SpectaclePrescription { get; set; }
        [LoadColumn(2)] [ColumnName("astigmatic")] public float Astigmatic { get; set; }
        [LoadColumn(3)] [ColumnName("class")] public float Class { get; set; }
    }
}
