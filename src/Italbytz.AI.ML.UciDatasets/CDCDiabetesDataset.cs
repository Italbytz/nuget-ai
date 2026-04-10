using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class CDCDiabetesDataset : Dataset<CDCDiabetesDataset.CDCDiabetesModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    public override string FilePrefix => "cdcd";

    public override string? LabelColumnName => "Diabetes_binary";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "HighBP", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "HighChol", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "CholCheck", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "BMI", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Smoker", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Stroke", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "HeartDiseaseorAttack", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "PhysActivity", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Fruits", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Veggies", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "HvyAlcoholConsump", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "AnyHealthcare", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "NoDocbcCost", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "GenHlth", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "MentHlth", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "PhysHlth", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "DiffWalk", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Sex", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Age", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Education", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Income", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Diabetes_binary", ColumnPurpose = ColumnPurposeType.Label, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true }
    ];

    public override IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFileInternal(path, separatorChar, hasHeader, allowQuoting, trimWhitespace, allowSparse);
    }

    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.ReplaceMissingValues(new[]
        {
            new InputOutputColumnPair("HighBP"),
            new InputOutputColumnPair("HighChol"),
            new InputOutputColumnPair("CholCheck"),
            new InputOutputColumnPair("BMI"),
            new InputOutputColumnPair("Smoker"),
            new InputOutputColumnPair("Stroke"),
            new InputOutputColumnPair("HeartDiseaseorAttack"),
            new InputOutputColumnPair("PhysActivity"),
            new InputOutputColumnPair("Fruits"),
            new InputOutputColumnPair("Veggies"),
            new InputOutputColumnPair("HvyAlcoholConsump"),
            new InputOutputColumnPair("AnyHealthcare"),
            new InputOutputColumnPair("NoDocbcCost"),
            new InputOutputColumnPair("GenHlth"),
            new InputOutputColumnPair("MentHlth"),
            new InputOutputColumnPair("PhysHlth"),
            new InputOutputColumnPair("DiffWalk"),
            new InputOutputColumnPair("Sex"),
            new InputOutputColumnPair("Age"),
            new InputOutputColumnPair("Education"),
            new InputOutputColumnPair("Income")
        });
    }

    protected override IEstimator<ITransformer>? BuildLabelMappingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Conversion
            .MapValueToKey("Diabetes_binary", "Diabetes_binary", addKeyValueAnnotationsAsText: false)
            .Append(mlContext.Transforms.CopyColumns(DefaultColumnNames.Label, "Diabetes_binary"));
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
            "HighBP", "HighChol", "CholCheck", "BMI", "Smoker", "Stroke", "HeartDiseaseorAttack", "PhysActivity", "Fruits", "Veggies",
            "HvyAlcoholConsump", "AnyHealthcare", "NoDocbcCost", "GenHlth", "MentHlth", "PhysHlth", "DiffWalk", "Sex", "Age", "Education", "Income");
    }

    public class CDCDiabetesModelInput
    {
        [LoadColumn(0)] [ColumnName("HighBP")] public float HighBP { get; set; }
        [LoadColumn(1)] [ColumnName("HighChol")] public float HighChol { get; set; }
        [LoadColumn(2)] [ColumnName("CholCheck")] public float CholCheck { get; set; }
        [LoadColumn(3)] [ColumnName("BMI")] public float BMI { get; set; }
        [LoadColumn(4)] [ColumnName("Smoker")] public float Smoker { get; set; }
        [LoadColumn(5)] [ColumnName("Stroke")] public float Stroke { get; set; }
        [LoadColumn(6)] [ColumnName("HeartDiseaseorAttack")] public float HeartDiseaseorAttack { get; set; }
        [LoadColumn(7)] [ColumnName("PhysActivity")] public float PhysActivity { get; set; }
        [LoadColumn(8)] [ColumnName("Fruits")] public float Fruits { get; set; }
        [LoadColumn(9)] [ColumnName("Veggies")] public float Veggies { get; set; }
        [LoadColumn(10)] [ColumnName("HvyAlcoholConsump")] public float HvyAlcoholConsump { get; set; }
        [LoadColumn(11)] [ColumnName("AnyHealthcare")] public float AnyHealthcare { get; set; }
        [LoadColumn(12)] [ColumnName("NoDocbcCost")] public float NoDocbcCost { get; set; }
        [LoadColumn(13)] [ColumnName("GenHlth")] public float GenHlth { get; set; }
        [LoadColumn(14)] [ColumnName("MentHlth")] public float MentHlth { get; set; }
        [LoadColumn(15)] [ColumnName("PhysHlth")] public float PhysHlth { get; set; }
        [LoadColumn(16)] [ColumnName("DiffWalk")] public float DiffWalk { get; set; }
        [LoadColumn(17)] [ColumnName("Sex")] public float Sex { get; set; }
        [LoadColumn(18)] [ColumnName("Age")] public float Age { get; set; }
        [LoadColumn(19)] [ColumnName("Education")] public float Education { get; set; }
        [LoadColumn(20)] [ColumnName("Income")] public float Income { get; set; }
        [LoadColumn(21)] [ColumnName("Diabetes_binary")] public float DiabetesBinary { get; set; }
    }
}
