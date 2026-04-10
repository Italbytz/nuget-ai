using System.Collections.Generic;

namespace Italbytz.AI.Learning.Framework;

public class Example : IExample
{
    private readonly IAttribute _targetAttribute;

    public Example(Dictionary<string, IAttribute> attributes, IAttribute targetAttribute)
    {
        Attributes = attributes;
        _targetAttribute = targetAttribute;
    }

    public Dictionary<string, IAttribute> Attributes { get; }

    public string TargetValue()
    {
        return GetAttributeValueAsString(_targetAttribute.Name());
    }

    public string GetAttributeValueAsString(string attributeName)
    {
        return Attributes[attributeName].ValueAsString();
    }
}
