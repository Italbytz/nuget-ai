namespace Italbytz.AI.Learning;

public interface IAttributeSpecification
{
    string AttributeName { get; }

    IAttribute CreateAttribute(string rawValue);

    bool IsValid(string value);
}
