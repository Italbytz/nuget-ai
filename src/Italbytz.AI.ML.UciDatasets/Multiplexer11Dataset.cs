using Italbytz.AI.ML.Core.Configuration;
using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class Multiplexer11Dataset : Dataset<
    Multiplexer11Dataset.Multiplexer11ModelInput>
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
            "ColumnName": "A_0",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "A_1",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "A_2",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "R_0",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "R_1",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "R_2",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "R_3",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "R_4",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "R_5",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "R_6",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "R_7",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "class",
            "ColumnPurpose": "Label",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          }
        ]
        """;

    protected override string ResourceName { get; } =
        "Italbytz.AI.ML.UciDatasets.Data.Multiplexer11.csv";

    public override string FilePrefix { get; } = "multiplexer11";

    public override string? LabelColumnName { get; } = @"class";


    public override IDataView LoadFromTextFile(string path,
        char? separatorChar = null,
        bool? hasHeader = null, bool? allowQuoting = null,
        bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFile<Multiplexer11ModelInput>(path, separatorChar,
            hasHeader,
            allowQuoting, trimWhitespace, allowSparse);
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(
        MLContext mlContext,
        ScenarioType scenarioType = ScenarioType.Classification,
        ProcessingType processingType = ProcessingType.Standard)
    {
        return mlContext.Transforms.Concatenate(@"Features", @"A_0", @"A_1",
            @"A_2", @"R_0", @"R_1", @"R_2", @"R_3", @"R_4", @"R_5", @"R_6",
            @"R_7");
    }

    protected override IEstimator<ITransformer>? BuildLabelMappingPipeline(
        MLContext mlContext,
        ScenarioType scenarioType = ScenarioType.Classification,
        ProcessingType processingType = ProcessingType.Standard)
    {
        return processingType switch
        {
            ProcessingType.FeatureBinningAndCustomLabelMapping => mlContext
                .Transforms.Conversion.MapValueToKey(@"Label", @"class",
                    keyData: mlContext.Data.LoadFromEnumerable(_lookupData)),
            ProcessingType.Standard => mlContext.Transforms.Conversion
                .MapValueToKey(@"class", @"class",
                    addKeyValueAnnotationsAsText: false)
                .Append(mlContext.Transforms.CopyColumns("Label", "class")),
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
            new InputOutputColumnPair(@"A_0"),
            new InputOutputColumnPair(@"A_1"),
            new InputOutputColumnPair(@"A_2"),
            new InputOutputColumnPair(@"R_0"),
            new InputOutputColumnPair(@"R_1"),
            new InputOutputColumnPair(@"R_2"),
            new InputOutputColumnPair(@"R_3"),
            new InputOutputColumnPair(@"R_4"),
            new InputOutputColumnPair(@"R_5"),
            new InputOutputColumnPair(@"R_6"),
            new InputOutputColumnPair(@"R_7")
        });
        return pipeline;
    }


    public class Multiplexer11ModelInput
    {
        [LoadColumn(0)] [ColumnName(@"A_0")] public float A_0 { get; set; }

        [LoadColumn(1)] [ColumnName(@"A_1")] public float A_1 { get; set; }

        [LoadColumn(2)] [ColumnName(@"A_2")] public float A_2 { get; set; }

        [LoadColumn(3)] [ColumnName(@"R_0")] public float R_0 { get; set; }

        [LoadColumn(4)] [ColumnName(@"R_1")] public float R_1 { get; set; }

        [LoadColumn(5)] [ColumnName(@"R_2")] public float R_2 { get; set; }

        [LoadColumn(6)] [ColumnName(@"R_3")] public float R_3 { get; set; }

        [LoadColumn(7)] [ColumnName(@"R_4")] public float R_4 { get; set; }

        [LoadColumn(8)] [ColumnName(@"R_5")] public float R_5 { get; set; }

        [LoadColumn(9)] [ColumnName(@"R_6")] public float R_6 { get; set; }

        [LoadColumn(10)] [ColumnName(@"R_7")] public float R_7 { get; set; }

        [LoadColumn(11)]
        [ColumnName(@"class")]
        public float Class { get; set; }
    }
}