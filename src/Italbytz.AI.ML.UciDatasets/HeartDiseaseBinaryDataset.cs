using Italbytz.AI.ML.Core.Configuration;
using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class
    HeartDiseaseBinaryDataset : Dataset<
    HeartDiseaseBinaryDataset.HeartDiseaseModelInput>
{
  protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext)
  {
    return AdditionalPreprocessingPipeline(mlContext, ScenarioType.Classification, ProcessingType.Standard);
  }

    private readonly LookupMap<float>[] _lookupData =
    [
        new(0.0f),
        new(1.0f)
    ];

    public override bool HasHeader { get; } = true;

    public override char Separator { get; } = ',';

    protected override string ColumnPropertiesString { get; } = """
        [
          {
            "ColumnName": "age",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "sex",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "cp",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "trestbps",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "chol",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "fbs",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "restecg",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "thalach",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "exang",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "oldpeak",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "slope",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "ca",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "thal",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "num",
            "ColumnPurpose": "Label",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          }
        ]
        """;

    protected override string ResourceName { get; } =
        "Italbytz.AI.ML.UciDatasets.Data.Heart_Disease_Binary.csv";

    public override string FilePrefix { get; } = "heart_disease_binary";

    public override string? LabelColumnName { get; } = @"num";


    public override IDataView LoadFromTextFile(string path,
        char? separatorChar = null,
        bool? hasHeader = null, bool? allowQuoting = null,
        bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFile<HeartDiseaseModelInput>(path, separatorChar,
            hasHeader,
            allowQuoting, trimWhitespace, allowSparse);
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(
        MLContext mlContext,
        ScenarioType scenarioType = ScenarioType.Classification,
        ProcessingType processingType = ProcessingType.Standard)
    {
        if (processingType ==
            ProcessingType.FeatureBinningAndCustomLabelMapping)
            return mlContext.Transforms.NormalizeBinning(new[]
            {
                new InputOutputColumnPair(@"age",
                    @"age"),
                new InputOutputColumnPair("trestbps",
                    "trestbps"),
                new InputOutputColumnPair(@"chol",
                    @"chol"),
                new InputOutputColumnPair(@"thalach",
                    @"thalach"),
                new InputOutputColumnPair(@"oldpeak",
                    @"oldpeak"),
                new InputOutputColumnPair(@"ca", @"ca")
            }, maximumBinCount: 4).Append(mlContext.Transforms.Concatenate(
                @"Features", @"age",
                @"sex", @"cp", @"trestbps", @"chol", @"fbs", @"restecg",
                @"thalach", @"exang", @"oldpeak", @"slope", @"ca", @"thal"));
        if (processingType == ProcessingType.Standard)
            return mlContext.Transforms.Concatenate(@"Features", @"age",
                @"sex", @"cp", @"trestbps", @"chol", @"fbs", @"restecg",
                @"thalach", @"exang", @"oldpeak", @"slope", @"ca", @"thal");
        throw new NotImplementedException();
    }

    protected override IEstimator<ITransformer>? BuildLabelMappingPipeline(
        MLContext mlContext,
        ScenarioType scenarioType = ScenarioType.Classification,
        ProcessingType processingType = ProcessingType.Standard)
    {
        return processingType switch
        {
            ProcessingType.FeatureBinningAndCustomLabelMapping => mlContext
                .Transforms.Conversion.MapValueToKey(@"Label", @"num",
                    keyData: mlContext.Data.LoadFromEnumerable(_lookupData)),
            ProcessingType.Standard => mlContext.Transforms.Conversion
                .MapValueToKey(@"num", @"num",
                    addKeyValueAnnotationsAsText: false)
                .Append(mlContext.Transforms.CopyColumns("Label", "num")),
            _ => throw new NotImplementedException()
        };
    }

    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(
        MLContext mlContext,
        ScenarioType scenarioType = ScenarioType.Classification,
        ProcessingType processingType = ProcessingType.Standard)
    {
        var pipeline = mlContext.Transforms.ReplaceMissingValues(new[]
        {
            new InputOutputColumnPair(@"age", @"age"),
            new InputOutputColumnPair(@"sex", @"sex"),
            new InputOutputColumnPair(@"cp", @"cp"),
            new InputOutputColumnPair(@"trestbps", @"trestbps"),
            new InputOutputColumnPair(@"chol", @"chol"),
            new InputOutputColumnPair(@"fbs", @"fbs"),
            new InputOutputColumnPair(@"restecg", @"restecg"),
            new InputOutputColumnPair(@"thalach", @"thalach"),
            new InputOutputColumnPair(@"exang", @"exang"),
            new InputOutputColumnPair(@"oldpeak", @"oldpeak"),
            new InputOutputColumnPair(@"slope", @"slope"),
            new InputOutputColumnPair(@"ca", @"ca"),
            new InputOutputColumnPair(@"thal", @"thal")
        });
        return pipeline;
    }


    public class HeartDiseaseModelInput
    {
        [LoadColumn(0)] [ColumnName(@"age")] public float Age { get; set; }

        [LoadColumn(1)] [ColumnName(@"sex")] public float Sex { get; set; }

        [LoadColumn(2)] [ColumnName(@"cp")] public float Cp { get; set; }

        [LoadColumn(3)]
        [ColumnName(@"trestbps")]
        public float Trestbps { get; set; }

        [LoadColumn(4)] [ColumnName(@"chol")] public float Chol { get; set; }

        [LoadColumn(5)] [ColumnName(@"fbs")] public float Fbs { get; set; }

        [LoadColumn(6)]
        [ColumnName(@"restecg")]
        public float Restecg { get; set; }

        [LoadColumn(7)]
        [ColumnName(@"thalach")]
        public float Thalach { get; set; }

        [LoadColumn(8)] [ColumnName(@"exang")] public float Exang { get; set; }

        [LoadColumn(9)]
        [ColumnName(@"oldpeak")]
        public float Oldpeak { get; set; }

        [LoadColumn(10)]
        [ColumnName(@"slope")]
        public float Slope { get; set; }

        [LoadColumn(11)] [ColumnName(@"ca")] public float Ca { get; set; }

        [LoadColumn(12)] [ColumnName(@"thal")] public float Thal { get; set; }

        [LoadColumn(13)] [ColumnName(@"num")] public float Num { get; set; }
    }
}