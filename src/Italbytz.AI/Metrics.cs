using System.Collections.Generic;
using System.Globalization;
using Italbytz.AI.Abstractions;

namespace Italbytz.AI;

/// <summary>
/// Default <see cref="IMetrics"/> implementation backed by an in-memory dictionary.
/// </summary>
public class Metrics : IMetrics
{
    private readonly Dictionary<string, string> _values = new();

    public string Get(string name)
    {
        return _values[name];
    }

    public void Set(string name, int value)
    {
        _values[name] = value.ToString(CultureInfo.InvariantCulture);
    }

    public void Set(string name, long value)
    {
        _values[name] = value.ToString(CultureInfo.InvariantCulture);
    }

    public void Set(string name, double value)
    {
        _values[name] = value.ToString(CultureInfo.InvariantCulture);
    }

    public void IncrementInt(string name)
    {
        Set(name, GetInt(name) + 1);
    }

    public int GetInt(string name)
    {
        return int.Parse(_values[name], CultureInfo.InvariantCulture);
    }

    public double GetDouble(string name)
    {
        return double.Parse(_values[name], CultureInfo.InvariantCulture);
    }

    public long GetLong(string name)
    {
        return long.Parse(_values[name], CultureInfo.InvariantCulture);
    }
}
