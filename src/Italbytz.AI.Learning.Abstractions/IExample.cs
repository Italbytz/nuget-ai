using System.Collections.Generic;

namespace Italbytz.AI.Learning;

public interface IExample
{
    Dictionary<string, IAttribute> Attributes { get; }

    string TargetValue();

    string GetAttributeValueAsString(string attributeName);
}
