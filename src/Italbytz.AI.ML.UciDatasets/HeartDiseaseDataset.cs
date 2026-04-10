using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class HeartDiseaseDataset : Dataset<HeartDiseaseDataset.HeartDiseaseModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    public override string FilePrefix => "heart_disease";

    public override string? LabelColumnName => "num";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "age", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "sex", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "cp", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "trestbps", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "chol", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "fbs", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "restecg", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "thalach", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "exang", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "oldpeak", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "slope", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "ca", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "thal", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "num", ColumnPurpose = ColumnPurposeType.Label, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true }
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
            new InputOutputColumnPair("sex"),
            new InputOutputColumnPair("cp"),
            new InputOutputColumnPair("trestbps"),
            new InputOutputColumnPair("chol"),
            new InputOutputColumnPair("fbs"),
            new InputOutputColumnPair("restecg"),
            new InputOutputColumnPair("thalach"),
            new InputOutputColumnPair("exang"),
            new InputOutputColumnPair("oldpeak"),
            new InputOutputColumnPair("slope"),
            new InputOutputColumnPair("ca"),
            new InputOutputColumnPair("thal")
        });
    }

    protected override IEstimator<ITransformer>? BuildLabelMappingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Conversion
            .MapValueToKey("num", "num", addKeyValueAnnotationsAsText: false)
            .Append(mlContext.Transforms.CopyColumns(DefaultColumnNames.Label, "num"));
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
            "age", "sex", "cp", "trestbps", "chol", "fbs", "restecg", "thalach", "exang", "oldpeak", "slope", "ca", "thal");
    }

    public class HeartDiseaseModelInput
    {
        [LoadColumn(0)] [ColumnName("age")] public float Age { get; set; }
        [LoadColumn(1)] [ColumnName("sex")] public float Sex { get; set; }
        [LoadColumn(2)] [ColumnName("cp")] public float Cp { get; set; }
        [LoadColumn(3)] [ColumnName("trestbps")] public float Trestbps { get; set; }
        [LoadColumn(4)] [ColumnName("chol")] public float Chol { get; set; }
        [LoadColumn(5)] [ColumnName("fbs")] public float Fbs { get; set; }
        [LoadColumn(6)] [ColumnName("restecg")] public float Restecg { get; set; }
        [LoadColumn(7)] [ColumnName("thalach")] public float Thalach { get; set; }
        [LoadColumn(8)] [ColumnName("exang")] public float Exang { get; set; }
        [LoadColumn(9)] [ColumnName("oldpeak")] public float Oldpeak { get; set; }
        [LoadColumn(10)] [ColumnName("slope")] public float Slope { get; set; }
        [LoadColumn(11)] [ColumnName("ca")] public float Ca { get; set; }
        [LoadColumn(12)] [ColumnName("thal")] public float Thal { get; set; }
        [LoadColumn(13)] [ColumnName("num")] public float Num { get; set; }
    }
}
