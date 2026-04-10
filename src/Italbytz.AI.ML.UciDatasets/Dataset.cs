using Italbytz.AI.ML.Core;
using Microsoft.ML;

namespace Italbytz.AI.ML.UciDatasets;

public abstract class Dataset<TModelInput> : IDataset
    where TModelInput : class, new()
{
    public abstract string? LabelColumnName { get; }

    public abstract ColumnPropertiesV5[] ColumnProperties { get; }

    public abstract string FilePrefix { get; }

    public virtual bool AllowQuoting => false;
    public virtual bool AllowSparse => false;
    public virtual char Separator => '\t';
    public virtual bool HasHeader => false;
    public virtual bool TrimWhitespace => false;

    public abstract IDataView LoadFromTextFile(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null);

    protected IDataView LoadFromTextFileInternal(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null)
    {
        return ThreadSafeMLContext.LocalMLContext.Data.LoadFromTextFile<TModelInput>(
            path,
            separatorChar ?? Separator,
            hasHeader ?? HasHeader,
            allowQuoting ?? AllowQuoting,
            trimWhitespace ?? TrimWhitespace,
            allowSparse ?? AllowSparse);
    }

    public IEstimator<ITransformer> BuildPipeline(MLContext mlContext, IEstimator<ITransformer> estimator)
    {
        return BuildPreprocessingPipeline(mlContext).Append(estimator);
    }

    public virtual IEstimator<ITransformer> BuildPreprocessingPipeline(MLContext mlContext)
    {
        var pipeline = AdditionalPreprocessingPipeline(mlContext);
        var labelMapping = BuildLabelMappingPipeline(mlContext);
        if (labelMapping != null)
        {
            pipeline = pipeline.Append(labelMapping);
        }

        var featurization = BuildFeaturizationPipeline(mlContext);
        if (featurization != null)
        {
            pipeline = pipeline.Append(featurization);
        }

        return pipeline;
    }

    protected abstract IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext);

    protected virtual IEstimator<ITransformer>? BuildLabelMappingPipeline(MLContext mlContext)
    {
        return null;
    }

    protected virtual IEstimator<ITransformer>? BuildFeaturizationPipeline(MLContext mlContext)
    {
        return null;
    }
}
