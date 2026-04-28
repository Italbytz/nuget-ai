namespace Italbytz.AI.Agent;

public class DynamicPercept : ObjectWithDynamicAttributes, IPercept
{
    public DynamicPercept SetAttribute(object key, object? value)
    {
        Attributes[key] = value;
        return this;
    }

    public object? GetAttribute(object key)
    {
        return Attributes.TryGetValue(key, out var value) ? value : null;
    }
}