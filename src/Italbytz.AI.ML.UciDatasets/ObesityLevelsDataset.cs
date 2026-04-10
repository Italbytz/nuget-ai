using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class ObesityLevelsDataset : Dataset<ObesityLevelsDataset.ObesityLevelsModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    public override string FilePrefix => "ol";

    public override string? LabelColumnName => "NObeyesdad";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "Gender", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "Age", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Height", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Weight", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "family_history_with_overweight", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "FAVC", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "FCVC", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "NCP", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "CAEC", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "SMOKE", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "CH2O", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "SCC", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "FAF", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "TUE", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "CALC", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "MTRANS", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true },
        new() { ColumnName = "NObeyesdad", ColumnPurpose = ColumnPurposeType.Label, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true }
    ];

    public override IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFileInternal(path, separatorChar, hasHeader, allowQuoting, trimWhitespace, allowSparse);
    }

    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.ReplaceMissingValues(new[]
        {
            new InputOutputColumnPair("Age"),
            new InputOutputColumnPair("Height"),
            new InputOutputColumnPair("Weight"),
            new InputOutputColumnPair("FCVC"),
            new InputOutputColumnPair("NCP"),
            new InputOutputColumnPair("CH2O"),
            new InputOutputColumnPair("FAF"),
            new InputOutputColumnPair("TUE")
        });
    }

    protected override IEstimator<ITransformer>? BuildLabelMappingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Conversion
            .MapValueToKey("NObeyesdad", "NObeyesdad", addKeyValueAnnotationsAsText: false)
            .Append(mlContext.Transforms.CopyColumns(DefaultColumnNames.Label, "NObeyesdad"));
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Categorical.OneHotEncoding(new[]
            {
                new InputOutputColumnPair("Gender"),
                new InputOutputColumnPair("family_history_with_overweight"),
                new InputOutputColumnPair("FAVC"),
                new InputOutputColumnPair("CAEC"),
                new InputOutputColumnPair("SMOKE"),
                new InputOutputColumnPair("SCC"),
                new InputOutputColumnPair("CALC"),
                new InputOutputColumnPair("MTRANS")
            })
            .Append(mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
                "Gender", "family_history_with_overweight", "FAVC", "CAEC", "SMOKE", "SCC", "CALC", "MTRANS",
                "Age", "Height", "Weight", "FCVC", "NCP", "CH2O", "FAF", "TUE"));
    }

    public class ObesityLevelsModelInput
    {
        [LoadColumn(0)] [ColumnName("Gender")] public string Gender { get; set; } = string.Empty;
        [LoadColumn(1)] [ColumnName("Age")] public float Age { get; set; }
        [LoadColumn(2)] [ColumnName("Height")] public float Height { get; set; }
        [LoadColumn(3)] [ColumnName("Weight")] public float Weight { get; set; }
        [LoadColumn(4)] [ColumnName("family_history_with_overweight")] public string FamilyHistoryWithOverweight { get; set; } = string.Empty;
        [LoadColumn(5)] [ColumnName("FAVC")] public string FAVC { get; set; } = string.Empty;
        [LoadColumn(6)] [ColumnName("FCVC")] public float FCVC { get; set; }
        [LoadColumn(7)] [ColumnName("NCP")] public float NCP { get; set; }
        [LoadColumn(8)] [ColumnName("CAEC")] public string CAEC { get; set; } = string.Empty;
        [LoadColumn(9)] [ColumnName("SMOKE")] public string SMOKE { get; set; } = string.Empty;
        [LoadColumn(10)] [ColumnName("CH2O")] public float CH2O { get; set; }
        [LoadColumn(11)] [ColumnName("SCC")] public string SCC { get; set; } = string.Empty;
        [LoadColumn(12)] [ColumnName("FAF")] public float FAF { get; set; }
        [LoadColumn(13)] [ColumnName("TUE")] public float TUE { get; set; }
        [LoadColumn(14)] [ColumnName("CALC")] public string CALC { get; set; } = string.Empty;
        [LoadColumn(15)] [ColumnName("MTRANS")] public string MTRANS { get; set; } = string.Empty;
        [LoadColumn(16)] [ColumnName("NObeyesdad")] public string NObeyesdad { get; set; } = string.Empty;
    }
}
