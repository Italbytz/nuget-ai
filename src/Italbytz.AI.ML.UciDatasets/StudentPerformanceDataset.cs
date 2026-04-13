using Italbytz.AI.ML.Core.Configuration;
using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class StudentPerformanceDataset : Dataset<
    StudentPerformanceDataset.StudentPerformanceModelInput>
{
    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext)
    {
        return AdditionalPreprocessingPipeline(mlContext, ScenarioType.Classification, ProcessingType.Standard);
    }

    public override bool HasHeader { get; } = true;

    public override char Separator { get; } = ',';

    protected override string ResourceName { get; } =
        "Italbytz.AI.ML.UciDatasets.Data.Student_Performance_G3.csv";

    public override string FilePrefix { get; } = "student";

    public override string? LabelColumnName { get; } = @"G3";

    protected override string ColumnPropertiesString { get; } = """
        [
          {
            "ColumnName": "school",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "sex",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "age",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "address",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "famsize",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "Pstatus",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "Medu",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "Fedu",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "Mjob",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "Fjob",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "reason",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "guardian",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "traveltime",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "studytime",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "failures",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "schoolsup",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Boolean",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "famsup",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Boolean",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "paid",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Boolean",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "activities",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Boolean",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "nursery",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Boolean",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "higher",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "internet",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Boolean",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "romantic",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Boolean",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "famrel",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "freetime",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "goout",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "Dalc",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "Walc",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "health",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "absences",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "G3",
            "ColumnPurpose": "Label",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          }
        ]
        """;

    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(
        MLContext mlContext,
        ScenarioType scenarioType = ScenarioType.Classification,
        ProcessingType processingType = ProcessingType.Standard)
    {
        return mlContext.Transforms.ReplaceMissingValues(new[]
        {
            new InputOutputColumnPair(@"age", @"age"),
            new InputOutputColumnPair(@"Medu", @"Medu"),
            new InputOutputColumnPair(@"Fedu", @"Fedu"),
            new InputOutputColumnPair(@"traveltime", @"traveltime"),
            new InputOutputColumnPair(@"studytime", @"studytime"),
            new InputOutputColumnPair(@"failures", @"failures"),
            new InputOutputColumnPair(@"famrel", @"famrel"),
            new InputOutputColumnPair(@"freetime", @"freetime"),
            new InputOutputColumnPair(@"goout", @"goout"),
            new InputOutputColumnPair(@"Dalc", @"Dalc"),
            new InputOutputColumnPair(@"Walc", @"Walc"),
            new InputOutputColumnPair(@"health", @"health"),
            new InputOutputColumnPair(@"absences", @"absences")
        });
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(
        MLContext mlContext,
        ScenarioType scenarioType = ScenarioType.Classification,
        ProcessingType processingType = ProcessingType.Standard)
    {
        return mlContext.Transforms.Categorical.OneHotEncoding(
                new[]
                {
                    new InputOutputColumnPair(@"school", @"school"),
                    new InputOutputColumnPair(@"sex", @"sex"),
                    new InputOutputColumnPair(@"address", @"address"),
                    new InputOutputColumnPair(@"famsize", @"famsize"),
                    new InputOutputColumnPair(@"Pstatus", @"Pstatus"),
                    new InputOutputColumnPair(@"Mjob", @"Mjob"),
                    new InputOutputColumnPair(@"Fjob", @"Fjob"),
                    new InputOutputColumnPair(@"reason", @"reason"),
                    new InputOutputColumnPair(@"guardian", @"guardian"),
                    new InputOutputColumnPair(@"schoolsup", @"schoolsup"),
                    new InputOutputColumnPair(@"famsup", @"famsup"),
                    new InputOutputColumnPair(@"paid", @"paid"),
                    new InputOutputColumnPair(@"activities", @"activities"),
                    new InputOutputColumnPair(@"nursery", @"nursery"),
                    new InputOutputColumnPair(@"higher", @"higher"),
                    new InputOutputColumnPair(@"internet", @"internet"),
                    new InputOutputColumnPair(@"romantic", @"romantic")
                })
            .Append(mlContext.Transforms.Concatenate(@"Features", @"school",
                @"sex", @"address", @"famsize", @"Pstatus", @"Mjob", @"Fjob",
                @"reason", @"guardian", @"schoolsup", @"famsup", @"paid",
                @"activities", @"nursery", @"higher", @"internet", @"romantic",
                @"age", @"Medu", @"Fedu", @"traveltime", @"studytime",
                @"failures", @"famrel", @"freetime", @"goout", @"Dalc", @"Walc",
                @"health", @"absences"));
    }

    public override IDataView LoadFromTextFile(string path,
        char? separatorChar = null,
        bool? hasHeader = null, bool? allowQuoting = null,
        bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFile<StudentPerformanceModelInput>(path,
            separatorChar,
            hasHeader,
            allowQuoting, trimWhitespace, allowSparse);
    }

    public class StudentPerformanceModelInput
    {
        [LoadColumn(0)]
        [ColumnName(@"school")]
        public string School { get; set; }

        [LoadColumn(1)] [ColumnName(@"sex")] public string Sex { get; set; }

        [LoadColumn(2)] [ColumnName(@"age")] public float Age { get; set; }

        [LoadColumn(3)]
        [ColumnName(@"address")]
        public string Address { get; set; }

        [LoadColumn(4)]
        [ColumnName(@"famsize")]
        public string Famsize { get; set; }

        [LoadColumn(5)]
        [ColumnName(@"Pstatus")]
        public string Pstatus { get; set; }

        [LoadColumn(6)] [ColumnName(@"Medu")] public float Medu { get; set; }

        [LoadColumn(7)] [ColumnName(@"Fedu")] public float Fedu { get; set; }

        [LoadColumn(8)] [ColumnName(@"Mjob")] public string Mjob { get; set; }

        [LoadColumn(9)] [ColumnName(@"Fjob")] public string Fjob { get; set; }

        [LoadColumn(10)]
        [ColumnName(@"reason")]
        public string Reason { get; set; }

        [LoadColumn(11)]
        [ColumnName(@"guardian")]
        public string Guardian { get; set; }

        [LoadColumn(12)]
        [ColumnName(@"traveltime")]
        public float Traveltime { get; set; }

        [LoadColumn(13)]
        [ColumnName(@"studytime")]
        public float Studytime { get; set; }

        [LoadColumn(14)]
        [ColumnName(@"failures")]
        public float Failures { get; set; }

        [LoadColumn(15)]
        [ColumnName(@"schoolsup")]
        public bool Schoolsup { get; set; }

        [LoadColumn(16)]
        [ColumnName(@"famsup")]
        public bool Famsup { get; set; }

        [LoadColumn(17)] [ColumnName(@"paid")] public bool Paid { get; set; }

        [LoadColumn(18)]
        [ColumnName(@"activities")]
        public bool Activities { get; set; }

        [LoadColumn(19)]
        [ColumnName(@"nursery")]
        public bool Nursery { get; set; }

        [LoadColumn(20)]
        [ColumnName(@"higher")]
        public string Higher { get; set; }

        [LoadColumn(21)]
        [ColumnName(@"internet")]
        public bool Internet { get; set; }

        [LoadColumn(22)]
        [ColumnName(@"romantic")]
        public bool Romantic { get; set; }

        [LoadColumn(23)]
        [ColumnName(@"famrel")]
        public float Famrel { get; set; }

        [LoadColumn(24)]
        [ColumnName(@"freetime")]
        public float Freetime { get; set; }

        [LoadColumn(25)]
        [ColumnName(@"goout")]
        public float Goout { get; set; }

        [LoadColumn(26)] [ColumnName(@"Dalc")] public float Dalc { get; set; }

        [LoadColumn(27)] [ColumnName(@"Walc")] public float Walc { get; set; }

        [LoadColumn(28)]
        [ColumnName(@"health")]
        public float Health { get; set; }

        [LoadColumn(29)]
        [ColumnName(@"absences")]
        public float Absences { get; set; }

        [LoadColumn(30)] [ColumnName(@"G3")] public float G3 { get; set; }
    }
}