using System;
using System.Collections.Generic;
using System.Linq;

namespace Italbytz.AI.Learning.Framework;

public class DataSetSpecification : IDataSetSpecification
{
    private readonly List<IAttributeSpecification> _attributeSpecifications = new();

    public string TargetAttribute { get; set; } = string.Empty;

    public bool IsValid(IEnumerable<string> uncheckedAttributes)
    {
        var attributes = uncheckedAttributes.ToList();
        if (_attributeSpecifications.Count != attributes.Count)
        {
            throw new InvalidOperationException($"size mismatch specsize = {_attributeSpecifications.Count} attributes size = {attributes.Count}");
        }

        return _attributeSpecifications.Zip(attributes, Tuple.Create)
            .All(pair => pair.Item1.IsValid(pair.Item2));
    }

    public IEnumerable<string> GetAttributeNames()
    {
        return _attributeSpecifications.Select(specification => specification.AttributeName).ToList();
    }

    public IAttributeSpecification GetAttributeSpecFor(string name)
    {
        var specification = _attributeSpecifications.FirstOrDefault(spec => spec.AttributeName.Equals(name, StringComparison.Ordinal));
        return specification ?? throw new InvalidOperationException($"no attribute spec for {name}");
    }

    public IEnumerable<string> GetPossibleAttributeValues(string attributeName)
    {
        var specification = _attributeSpecifications.FirstOrDefault(spec => spec.AttributeName.Equals(attributeName, StringComparison.Ordinal));
        if (specification is StringAttributeSpecification stringSpecification)
        {
            return stringSpecification.AttributePossibleValues;
        }

        throw new InvalidOperationException($"No such attribute {attributeName}");
    }

    public void DefineStringAttribute(string name, IEnumerable<string> attributeValues)
    {
        _attributeSpecifications.Add(new StringAttributeSpecification(name, attributeValues));
        TargetAttribute = name;
    }

    public void DefineNumericAttribute(string name)
    {
        _attributeSpecifications.Add(new NumericAttributeSpecification(name));
        TargetAttribute = name;
    }
}
