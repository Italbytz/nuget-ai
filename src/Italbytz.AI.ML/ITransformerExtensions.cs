using Microsoft.ML;
using Microsoft.ML.Data;

namespace Italbytz.AI.ML;

public static class ITransformerExtensions
{
    public static IPredictionTransformer<ICanSaveModel>? ExtractIPredictionTransformer(this ITransformer transformer)
    {
        if (transformer is IPredictionTransformer<ICanSaveModel> predictionTransformer)
        {
            return predictionTransformer;
        }

        if (transformer is IEnumerable<ITransformer> chain)
        {
            foreach (var item in chain)
            {
                var extracted = item.ExtractIPredictionTransformer();
                if (extracted != null)
                {
                    return extracted;
                }
            }
        }

        return null;
    }

    public static ICanSaveModel GetModelParameters(this ITransformer transformer)
    {
        var predictionTransformer = transformer.ExtractIPredictionTransformer();
        if (predictionTransformer == null)
        {
            throw new InvalidOperationException(
                "The transformer does not expose ML.NET prediction model parameters.");
        }

        return predictionTransformer.Model;
    }
}
