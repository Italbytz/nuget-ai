using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Italbytz.AI.ML.Trainers;

namespace Italbytz.AI.ML;

public static class ITransformerExtensions
{
    public static ICanSaveModel? ExtractModelParameters(this ITransformer transformer)
    {
        if (transformer is IEnumerable<ITransformer> chain)
        {
            ICanSaveModel? lastExtracted = null;
            foreach (var item in chain)
            {
                var extracted = item.ExtractModelParameters();
                if (extracted != null)
                {
                    lastExtracted = extracted;
                }
            }

            if (lastExtracted != null)
            {
                return lastExtracted;
            }
        }

        foreach (var iface in transformer.GetType().GetInterfaces())
        {
            if (!iface.IsGenericType || iface.GetGenericTypeDefinition() != typeof(IPredictionTransformer<>))
            {
                continue;
            }

            var modelProperty = iface.GetProperty("Model");
            if (modelProperty?.GetValue(transformer) is ICanSaveModel model)
            {
                return model;
            }
        }

        return null;
    }

    public static ICanSaveModel GetModelParameters(this ITransformer transformer)
    {
        var model = transformer.ExtractModelParameters();
        if (model == null)
        {
            throw new InvalidOperationException(
                "The transformer does not expose ML.NET prediction model parameters.");
        }

        if (model is OneVersusAllModelParameters oneVersusAllModelParameters)
        {
            model = oneVersusAllModelParameters.ToPublic();
        }

        return model;
    }
}
