using System.Collections.Generic;
using System.Text;

namespace Italbytz.AI.Agent;

/// <summary>
/// Base class for simple agent helper types with dynamic attributes.
/// </summary>
public abstract class ObjectWithDynamicAttributes
{
    protected Dictionary<object, object?> Attributes { get; } = new();

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(GetType().Name);
        sb.Append('[');

        var first = true;
        foreach (var attribute in Attributes)
        {
            if (!first)
            {
                sb.Append(", ");
            }

            first = false;
            sb.Append($"{attribute.Key}={attribute.Value}");
        }

        sb.Append(']');
        return sb.ToString();
    }
}
