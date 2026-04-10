namespace Italbytz.AI.Learning.Framework;

public class StringAttribute : IAttribute
{
    private readonly IAttributeSpecification _specification;
    private readonly string _value;

    public StringAttribute(string value, IAttributeSpecification specification)
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
        return _value.Trim();
    }
}
