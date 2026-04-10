using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class NPHADataset : Dataset<NPHADataset.NPHAModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    public override string FilePrefix => "npha";

    public override string? LabelColumnName => "Number_of_Doctors_Visited";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "Age", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Physical_Health", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Mental_Health", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Dental_Health", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "Employment", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Stress_Keeps_Patient_from_Sleeping", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Medication_Keeps_Patient_from_Sleeping", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Pain_Keeps_Patient_from_Sleeping", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Bathroom_Needs_Keeps_Patient_from_Sleeping", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Uknown_Keeps_Patient_from_Sleeping", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Trouble_Sleeping", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Prescription_Sleep_Medication", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Race", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Gender", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true },
        new() { ColumnName = "Number_of_Doctors_Visited", ColumnPurpose = ColumnPurposeType.Label, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = true }
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
            new InputOutputColumnPair("Physical_Health"),
            new InputOutputColumnPair("Mental_Health"),
            new InputOutputColumnPair("Dental_Health"),
            new InputOutputColumnPair("Employment"),
            new InputOutputColumnPair("Stress_Keeps_Patient_from_Sleeping"),
            new InputOutputColumnPair("Medication_Keeps_Patient_from_Sleeping"),
            new InputOutputColumnPair("Pain_Keeps_Patient_from_Sleeping"),
            new InputOutputColumnPair("Bathroom_Needs_Keeps_Patient_from_Sleeping"),
            new InputOutputColumnPair("Uknown_Keeps_Patient_from_Sleeping"),
            new InputOutputColumnPair("Trouble_Sleeping"),
            new InputOutputColumnPair("Prescription_Sleep_Medication"),
            new InputOutputColumnPair("Race"),
            new InputOutputColumnPair("Gender")
        });
    }

    protected override IEstimator<ITransformer>? BuildLabelMappingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Conversion
            .MapValueToKey("Number_of_Doctors_Visited", "Number_of_Doctors_Visited", addKeyValueAnnotationsAsText: false)
            .Append(mlContext.Transforms.CopyColumns(DefaultColumnNames.Label, "Number_of_Doctors_Visited"));
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.Concatenate(DefaultColumnNames.Features,
            "Age",
            "Physical_Health",
            "Mental_Health",
            "Dental_Health",
            "Employment",
            "Stress_Keeps_Patient_from_Sleeping",
            "Medication_Keeps_Patient_from_Sleeping",
            "Pain_Keeps_Patient_from_Sleeping",
            "Bathroom_Needs_Keeps_Patient_from_Sleeping",
            "Uknown_Keeps_Patient_from_Sleeping",
            "Trouble_Sleeping",
            "Prescription_Sleep_Medication",
            "Race",
            "Gender");
    }

    public class NPHAModelInput
    {
        [LoadColumn(0)] [ColumnName("Age")] public float Age { get; set; }
        [LoadColumn(1)] [ColumnName("Physical_Health")] public float PhysicalHealth { get; set; }
        [LoadColumn(2)] [ColumnName("Mental_Health")] public float MentalHealth { get; set; }
        [LoadColumn(3)] [ColumnName("Dental_Health")] public float DentalHealth { get; set; }
        [LoadColumn(4)] [ColumnName("Employment")] public float Employment { get; set; }
        [LoadColumn(5)] [ColumnName("Stress_Keeps_Patient_from_Sleeping")] public float StressKeepsPatientFromSleeping { get; set; }
        [LoadColumn(6)] [ColumnName("Medication_Keeps_Patient_from_Sleeping")] public float MedicationKeepsPatientFromSleeping { get; set; }
        [LoadColumn(7)] [ColumnName("Pain_Keeps_Patient_from_Sleeping")] public float PainKeepsPatientFromSleeping { get; set; }
        [LoadColumn(8)] [ColumnName("Bathroom_Needs_Keeps_Patient_from_Sleeping")] public float BathroomNeedsKeepsPatientFromSleeping { get; set; }
        [LoadColumn(9)] [ColumnName("Uknown_Keeps_Patient_from_Sleeping")] public float UknownKeepsPatientFromSleeping { get; set; }
        [LoadColumn(10)] [ColumnName("Trouble_Sleeping")] public float TroubleSleeping { get; set; }
        [LoadColumn(11)] [ColumnName("Prescription_Sleep_Medication")] public float PrescriptionSleepMedication { get; set; }
        [LoadColumn(12)] [ColumnName("Race")] public float Race { get; set; }
        [LoadColumn(13)] [ColumnName("Gender")] public float Gender { get; set; }
        [LoadColumn(14)] [ColumnName("Number_of_Doctors_Visited")] public float NumberOfDoctorsVisited { get; set; }
    }
}
