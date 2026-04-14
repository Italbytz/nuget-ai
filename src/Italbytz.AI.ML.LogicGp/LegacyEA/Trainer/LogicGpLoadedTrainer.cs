using Microsoft.ML;

namespace Italbytz.AI.ML.LogicGp.Internal.Trainer;

internal class LogicGpLoadedTrainer<TOutput> : global::Italbytz.AI.ML.LogicGp.LogicGpTrainer<TOutput>
    where TOutput : class, new()
{
    protected override void PrepareForFit(IDataView input)
    {
        // Intentionally do nothing.
    }
}