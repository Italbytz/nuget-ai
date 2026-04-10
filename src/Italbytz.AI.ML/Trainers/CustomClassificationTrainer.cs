using Italbytz.AI.ML.Core;

namespace Italbytz.AI.ML.Trainers;

public abstract class CustomClassificationTrainer<TOutput> : CustomTrainer<ClassificationInput, TOutput>
    where TOutput : class, new()
{
}
