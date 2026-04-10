namespace Italbytz.AI.ML.Core.Configuration;

public abstract class MBConfig
{
    public abstract int Version { get; }

    public virtual string? Type { get; set; }
}
