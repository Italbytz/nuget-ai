namespace Italbytz.AI.Abstractions;

/// <summary>
/// Stores key-value pairs for efficiency analysis and algorithm diagnostics.
/// </summary>
public interface IMetrics
{
    string Get(string name);

    void Set(string name, int value);

    void Set(string name, long value);

    void Set(string name, double value);

    void IncrementInt(string name);

    int GetInt(string name);

    double GetDouble(string name);

    long GetLong(string name);
}
