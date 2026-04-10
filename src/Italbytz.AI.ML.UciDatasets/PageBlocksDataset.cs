using Italbytz.AI.ML.Core;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML.UciDatasets;

public class PageBlocksDataset : Dataset<PageBlocksDataset.PageBlocksModelInput>
{
    public override bool HasHeader => true;

    public override char Separator => ',';

    public override string FilePrefix => "page_blocks_classification";

    public override string? LabelColumnName => "class";

    public override ColumnPropertiesV5[] ColumnProperties =>
    [
        new() { ColumnName = "height", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "length", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "area", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "eccen", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "p_black", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "p_and", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "mean_tr", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "blackpix", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "blackand", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
        new() { ColumnName = "wb_trans", ColumnPurpose = ColumnPurposeType.Feature, ColumnDataFormat = ColumnDataKind.Single, IsCategorical = false },
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
            new InputOutputColumnPair("height"),
            new InputOutputColumnPair("length"),
            new InputOutputColumnPair("area"),
            new InputOutputColumnPair("eccen"),
            new InputOutputColumnPair("p_black"),
            new InputOutputColumnPair("p_and"),
            new InputOutputColumnPair("mean_tr"),
            new InputOutputColumnPair("blackpix"),
            new InputOutputColumnPair("blackand"),
            new InputOutputColumnPair("wb_trans")
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
            "height", "length", "area", "eccen", "p_black", "p_and", "mean_tr", "blackpix", "blackand", "wb_trans");
    }

    public class PageBlocksModelInput
    {
        [LoadColumn(0)] [ColumnName("height")] public float Height { get; set; }
        [LoadColumn(1)] [ColumnName("length")] public float Length { get; set; }
        [LoadColumn(2)] [ColumnName("area")] public float Area { get; set; }
        [LoadColumn(3)] [ColumnName("eccen")] public float Eccen { get; set; }
        [LoadColumn(4)] [ColumnName("p_black")] public float PBlack { get; set; }
        [LoadColumn(5)] [ColumnName("p_and")] public float PAnd { get; set; }
        [LoadColumn(6)] [ColumnName("mean_tr")] public float MeanTr { get; set; }
        [LoadColumn(7)] [ColumnName("blackpix")] public float Blackpix { get; set; }
        [LoadColumn(8)] [ColumnName("blackand")] public float Blackand { get; set; }
        [LoadColumn(9)] [ColumnName("wb_trans")] public float WbTrans { get; set; }
        [LoadColumn(10)] [ColumnName("class")] public float Class { get; set; }
    }
}
