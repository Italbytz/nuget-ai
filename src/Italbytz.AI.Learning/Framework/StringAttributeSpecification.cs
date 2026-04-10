using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Learning.Framework;

public class StringAttributeSpecification : IAttributeSpecification
{
    public StringAttributeSpecification(string attributeName, IEnumerable<string> attributePossibleValues)
    {
        AttributeName = attributeName;
        AttributePossibleValues = attributePossibleValues.ToArray();
    }

    public IReadOnlyList<string> AttributePossibleValues { get; }

    public string AttributeName { get; }

    public IAttribute CreateAttribute(string rawValue)
    {
        return new StringAttribute(rawValue, this);
    }

    public bool IsValid(string value)
    {
        return AttributePossibleValues.Contains(value);
    }
}
