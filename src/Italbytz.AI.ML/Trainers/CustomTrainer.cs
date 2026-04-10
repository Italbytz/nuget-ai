using Microsoft.ML;
using Microsoft.ML.Transforms;

namespace Italbytz.AI.ML.Trainers;

public abstract class CustomTrainer<TInput, TOutput> : IEstimator<ITransformer>
    where TInput : class, new()
    where TOutput : class, new()
{
    public SchemaShape GetOutputSchema(SchemaShape inputSchema)
    {
        return GetCustomMappingEstimator().GetOutputSchema(inputSchema);
    }

    public ITransformer Fit(IDataView input)
    {
        PrepareForFit(input);
        return GetCustomMappingEstimator().Fit(input);
    }

    protected virtual void PrepareForFit(IDataView input)
    {
    }

    protected abstract void Map(TInput input, TOutput output);

    private CustomMappingEstimator<TInput, TOutput> GetCustomMappingEstimator()
    {
        return Core.ThreadSafeMLContext.LocalMLContext.Transforms.CustomMapping<TInput, TOutput>(Map, null);
    }
}
