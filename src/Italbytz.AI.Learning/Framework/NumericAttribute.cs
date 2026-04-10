using System.Globalization;

namespace Italbytz.AI.Learning.Framework;

public class NumericAttribute : IAttribute
{
    private readonly IAttributeSpecification _specification;
    private readonly double _value;

    public NumericAttribute(double value, IAttributeSpecification specification)
    {
        _value = value;
        _specification = specification;
    }

    public string Name()
    {
        return _specification.AttributeName.Trim();
    }

    public string ValueAsString()
    {
        return _value.ToString(CultureInfo.InvariantCulture);
    }

    public double ValueAsDouble()
    {
        return _value;
    }
}
