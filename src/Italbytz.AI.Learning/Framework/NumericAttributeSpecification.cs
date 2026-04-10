using System.Globalization;

namespace Italbytz.AI.Learning.Framework;

public class NumericAttributeSpecification : IAttributeSpecification
{
    public NumericAttributeSpecification(string attributeName)
    {
        AttributeName = attributeName;
    }

    public string AttributeName { get; }

    public IAttribute CreateAttribute(string rawValue)
    {
        return new NumericAttribute(double.Parse(rawValue, CultureInfo.InvariantCulture), this);
    }

    public bool IsValid(string value)
    {
        return double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out _);
    }
}
