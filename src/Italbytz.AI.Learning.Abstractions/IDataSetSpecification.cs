using System.Collections.Generic;

namespace Italbytz.AI.Learning;

public interface IDataSetSpecification
{
    string TargetAttribute { get; }

    bool IsValid(IEnumerable<string> uncheckedAttributes);

    IEnumerable<string> GetAttributeNames();

    IAttributeSpecification GetAttributeSpecFor(string name);

    IEnumerable<string> GetPossibleAttributeValues(string attributeName);
}
