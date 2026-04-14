using System.IO;

namespace Italbytz.AI.ML.LogicGp;

public static class LogicGp
{
    public static LogicGpTrainer<TOutput>? LoadTrainer<TOutput>(Stream stream)
        where TOutput : class, new()
    {
        return LogicGpTrainer<TOutput>.Load(stream);
    }
}