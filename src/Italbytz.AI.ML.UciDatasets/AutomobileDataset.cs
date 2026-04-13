using Italbytz.AI.ML.Core.Configuration;
using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class AutomobileDataset : Dataset<AutomobileDataset.AutomobileModelInput>
{
    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext)
    {
        return AdditionalPreprocessingPipeline(mlContext, ScenarioType.Classification, ProcessingType.Standard);
    }

    public override bool HasHeader { get; } = true;

    public override char Separator { get; } = ',';

    protected override string ResourceName { get; } =
        "Italbytz.AI.ML.UciDatasets.Data.Automobile.csv";

    public override string FilePrefix { get; } = "automobile";

    public override string? LabelColumnName { get; } = @"symboling";

    protected override string ColumnPropertiesString { get; } = """
        [
          {
            "ColumnName": "price",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "highway-mpg",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "city-mpg",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "peak-rpm",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "horsepower",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "compression-ratio",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "stroke",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "bore",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "fuel-system",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "engine-size",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "num-of-cylinders",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "engine-type",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "curb-weight",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "height",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "width",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "length",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "wheel-base",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "engine-location",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "drive-wheels",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "body-style",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "num-of-doors",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "aspiration",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "fuel-type",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "make",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "String",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "normalized-losses",
            "ColumnPurpose": "Feature",
            "ColumnDataFormat": "Single",
            "IsCategorical": false,
            "Type": "Column",
            "Version": 5
          },
          {
            "ColumnName": "symboling",
            "ColumnPurpose": "Label",
            "ColumnDataFormat": "Single",
            "IsCategorical": true,
            "Type": "Column",
            "Version": 5
          }
        ]
        """;


    public override IDataView LoadFromTextFile(string path,
        char? separatorChar = null,
        bool? hasHeader = null, bool? allowQuoting = null,
        bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFile<AutomobileModelInput>(path,
            separatorChar,
            hasHeader,
            allowQuoting, trimWhitespace, allowSparse);
    }

    protected override IEstimator<ITransformer>? BuildFeaturizationPipeline(
        MLContext mlContext,
        ScenarioType scenarioType = ScenarioType.Classification,
        ProcessingType processingType = ProcessingType.Standard)
    {
        return mlContext.Transforms.Categorical.OneHotEncoding(
                new[]
                {
                    new InputOutputColumnPair(@"fuel-system", @"fuel-system"),
                    new InputOutputColumnPair(@"engine-type", @"engine-type"),
                    new InputOutputColumnPair(@"engine-location",
                        @"engine-location"),
                    new InputOutputColumnPair(@"drive-wheels", @"drive-wheels"),
                    new InputOutputColumnPair(@"body-style", @"body-style"),
                    new InputOutputColumnPair(@"aspiration", @"aspiration"),
                    new InputOutputColumnPair(@"fuel-type", @"fuel-type")
                }).Append(mlContext.Transforms.Text.FeaturizeText(
                inputColumnName: @"make", outputColumnName: @"make"))
            .Append(mlContext.Transforms.Concatenate(@"Features",
                @"fuel-system", @"engine-type", @"engine-location",
                @"drive-wheels", @"body-style", @"aspiration", @"fuel-type",
                @"price", @"highway-mpg", @"city-mpg", @"peak-rpm",
                @"horsepower", @"compression-ratio", @"stroke", @"bore",
                @"engine-size", @"num-of-cylinders", @"curb-weight", @"height",
                @"width", @"length", @"wheel-base", @"num-of-doors",
                @"normalized-losses", @"make"));
    }

    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(
        MLContext mlContext,
        ScenarioType scenarioType = ScenarioType.Classification,
        ProcessingType processingType = ProcessingType.Standard)
    {
        var pipeline =
                mlContext.Transforms.ReplaceMissingValues(new[]
                {
                    new InputOutputColumnPair(@"price", @"price"),
                    new InputOutputColumnPair(@"highway-mpg", @"highway-mpg"),
                    new InputOutputColumnPair(@"city-mpg", @"city-mpg"),
                    new InputOutputColumnPair(@"peak-rpm", @"peak-rpm"),
                    new InputOutputColumnPair(@"horsepower", @"horsepower"),
                    new InputOutputColumnPair(@"compression-ratio",
                        @"compression-ratio"),
                    new InputOutputColumnPair(@"stroke", @"stroke"),
                    new InputOutputColumnPair(@"bore", @"bore"),
                    new InputOutputColumnPair(@"engine-size", @"engine-size"),
                    new InputOutputColumnPair(@"num-of-cylinders",
                        @"num-of-cylinders"),
                    new InputOutputColumnPair(@"curb-weight", @"curb-weight"),
                    new InputOutputColumnPair(@"height", @"height"),
                    new InputOutputColumnPair(@"width", @"width"),
                    new InputOutputColumnPair(@"length", @"length"),
                    new InputOutputColumnPair(@"wheel-base", @"wheel-base"),
                    new InputOutputColumnPair(@"num-of-doors", @"num-of-doors"),
                    new InputOutputColumnPair(@"normalized-losses",
                        @"normalized-losses")
                })
            ;

        return pipeline;
    }

    public class AutomobileModelInput
    {
        [LoadColumn(0)] [ColumnName(@"price")] public float Price { get; set; }

        [LoadColumn(1)]
        [ColumnName(@"highway-mpg")]
        public float Highway_mpg { get; set; }

        [LoadColumn(2)]
        [ColumnName(@"city-mpg")]
        public float City_mpg { get; set; }

        [LoadColumn(3)]
        [ColumnName(@"peak-rpm")]
        public float Peak_rpm { get; set; }

        [LoadColumn(4)]
        [ColumnName(@"horsepower")]
        public float Horsepower { get; set; }

        [LoadColumn(5)]
        [ColumnName(@"compression-ratio")]
        public float Compression_ratio { get; set; }

        [LoadColumn(6)]
        [ColumnName(@"stroke")]
        public float Stroke { get; set; }

        [LoadColumn(7)] [ColumnName(@"bore")] public float Bore { get; set; }

        [LoadColumn(8)]
        [ColumnName(@"fuel-system")]
        public string Fuel_system { get; set; }

        [LoadColumn(9)]
        [ColumnName(@"engine-size")]
        public float Engine_size { get; set; }

        [LoadColumn(10)]
        [ColumnName(@"num-of-cylinders")]
        public float Num_of_cylinders { get; set; }

        [LoadColumn(11)]
        [ColumnName(@"engine-type")]
        public string Engine_type { get; set; }

        [LoadColumn(12)]
        [ColumnName(@"curb-weight")]
        public float Curb_weight { get; set; }

        [LoadColumn(13)]
        [ColumnName(@"height")]
        public float Height { get; set; }

        [LoadColumn(14)]
        [ColumnName(@"width")]
        public float Width { get; set; }

        [LoadColumn(15)]
        [ColumnName(@"length")]
        public float Length { get; set; }

        [LoadColumn(16)]
        [ColumnName(@"wheel-base")]
        public float Wheel_base { get; set; }

        [LoadColumn(17)]
        [ColumnName(@"engine-location")]
        public string Engine_location { get; set; }

        [LoadColumn(18)]
        [ColumnName(@"drive-wheels")]
        public string Drive_wheels { get; set; }

        [LoadColumn(19)]
        [ColumnName(@"body-style")]
        public string Body_style { get; set; }

        [LoadColumn(20)]
        [ColumnName(@"num-of-doors")]
        public float Num_of_doors { get; set; }

        [LoadColumn(21)]
        [ColumnName(@"aspiration")]
        public string Aspiration { get; set; }

        [LoadColumn(22)]
        [ColumnName(@"fuel-type")]
        public string Fuel_type { get; set; }

        [LoadColumn(23)] [ColumnName(@"make")] public string Make { get; set; }

        [LoadColumn(24)]
        [ColumnName(@"normalized-losses")]
        public float Normalized_losses { get; set; }

        [LoadColumn(25)]
        [ColumnName(@"symboling")]
        public float Symboling { get; set; }
    }
}