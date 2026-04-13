using System.Text.Json;
using System.Text.Json.Serialization;
using Italbytz.AI.ML.Core;
using Italbytz.AI.ML.Core.Configuration;
using Microsoft.ML;

namespace Italbytz.AI.ML.UciDatasets;

public abstract class Dataset<TModelInput> : IDataset
    where TModelInput : class, new()
{
    private ColumnPropertiesV5[]? _columnPropertiesCache;
    private IDataView? _dataViewCache;

    public abstract string? LabelColumnName { get; }

    public virtual ColumnPropertiesV5[] ColumnProperties => _columnPropertiesCache ??= GetColumnProperties();

    public abstract string FilePrefix { get; }

    public virtual bool AllowQuoting => false;
    public virtual bool AllowSparse => false;
    public virtual char Separator => '\t';
    public virtual bool HasHeader => false;
    public virtual bool TrimWhitespace => false;

    // Legacy compatibility hooks for migrated datasets.
    protected virtual string? ColumnPropertiesString => null;
    protected virtual string? ResourceName => null;

    public virtual IDataView DataView => _dataViewCache ??= LoadEmbeddedResourceDataView();

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

    protected IDataView LoadFromTextFile<TCompatibleModelInput>(string path, char? separatorChar = null, bool? hasHeader = null, bool? allowQuoting = null, bool? trimWhitespace = null, bool? allowSparse = null)
        where TCompatibleModelInput : class, new()
    {
        return ThreadSafeMLContext.LocalMLContext.Data.LoadFromTextFile<TCompatibleModelInput>(
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

    public virtual IEstimator<ITransformer> BuildPipeline(MLContext mlContext, IEstimator<ITransformer> estimator, ScenarioType scenarioType, ProcessingType processingType)
    {
        return BuildPreprocessingPipeline(mlContext, scenarioType, processingType).Append(estimator);
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

    public virtual IEstimator<ITransformer> BuildPreprocessingPipeline(MLContext mlContext, ScenarioType scenarioType, ProcessingType processingType)
    {
        var pipeline = AdditionalPreprocessingPipeline(mlContext, scenarioType, processingType);
        var labelMapping = BuildLabelMappingPipeline(mlContext, scenarioType, processingType);
        if (labelMapping != null)
        {
            pipeline = pipeline.Append(labelMapping);
        }

        var featurization = BuildFeaturizationPipeline(mlContext, scenarioType, processingType);
        if (featurization != null)
        {
            pipeline = pipeline.Append(featurization);
        }

        return pipeline;
    }

    protected abstract IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext);

    protected virtual IEstimator<ITransformer> AdditionalPreprocessingPipeline(MLContext mlContext, ScenarioType scenarioType, ProcessingType processingType)
    {
        return AdditionalPreprocessingPipeline(mlContext);
    }

    protected virtual IEstimator<ITransformer>? BuildLabelMappingPipeline(MLContext mlContext)
    {
        return null;
    }

    protected virtual IEstimator<ITransformer>? BuildLabelMappingPipeline(MLContext mlContext, ScenarioType scenarioType, ProcessingType processingType)
    {
        return BuildLabelMappingPipeline(mlContext);
    }

    protected virtual IEstimator<ITransformer>? BuildFeaturizationPipeline(MLContext mlContext)
    {
        return null;
    }

    protected virtual IEstimator<ITransformer>? BuildFeaturizationPipeline(MLContext mlContext, ScenarioType scenarioType, ProcessingType processingType)
    {
        return BuildFeaturizationPipeline(mlContext);
    }

    private ColumnPropertiesV5[] GetColumnProperties()
    {
        if (string.IsNullOrWhiteSpace(ColumnPropertiesString))
        {
            return [];
        }

        var options = new JsonSerializerOptions
        {
            Converters = { new JsonStringEnumConverter() }
        };

        return JsonSerializer.Deserialize<ColumnPropertiesV5[]>(ColumnPropertiesString, options) ?? [];
    }

    private IDataView LoadEmbeddedResourceDataView()
    {
        if (string.IsNullOrWhiteSpace(ResourceName))
        {
            throw new InvalidOperationException("No embedded resource configured for this dataset.");
        }

        var assembly = typeof(Dataset<TModelInput>).Assembly;
        using var stream = assembly.GetManifestResourceStream(ResourceName)
                           ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");

        var tempFile = Path.GetTempFileName();
        using (var fileStream = File.Create(tempFile))
        {
            stream.CopyTo(fileStream);
        }

        return LoadFromTextFile(tempFile);
    }
}
