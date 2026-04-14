using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class IrisDataset : Dataset<IrisDataset.IrisModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    protected override string ResourceName =>
        "Italbytz.AI.ML.UciDatasets.Data.Iris.csv";

    public override string FilePrefix => "iris";

    public override string? LabelColumnName => "class";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "sepal length", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "sepal width", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "petal length", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "petal width", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "class", ColumnPurpose = ColumnPurposeType.Label, ColumnDataFormat = ColumnDataKind.String, IsCategorical = true }
    ];

    public override IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return LoadFromTextFileInternal(path, separatorChar, hasHeader, allowQuoting, trimWhitespace, allowSparse);
    }

    protected override IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext)
    {
        return mlContext.Transforms.ReplaceMissingValues(new[]
        {
            new InputOutputColumnPair("sepal length"),
            new InputOutputColumnPair("sepal width"),
            new InputOutputColumnPair("petal length"),
            new InputOutputColumnPair("petal width")
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
        return mlContext.Transforms.Concatenate(DefaultColumnNames.Features, "sepal length", "sepal width", "petal length", "petal width");
    }

    public class IrisModelInput
    {
        [LoadColumn(0)]
        [ColumnName("sepal length")]
        public float SepalLength { get; set; }

        [LoadColumn(1)]
        [ColumnName("sepal width")]
        public float SepalWidth { get; set; }

        [LoadColumn(2)]
        [ColumnName("petal length")]
        public float PetalLength { get; set; }

        [LoadColumn(3)]
        [ColumnName("petal width")]
        public float PetalWidth { get; set; }

        [LoadColumn(4)]
        [ColumnName("class")]
        public string Class { get; set; } = string.Empty;
    }
}
