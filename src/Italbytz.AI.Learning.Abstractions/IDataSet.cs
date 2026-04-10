using System.Collections.Generic;

namespace Italbytz.AI.Learning;

public interface IDataSet : IEnumerable<IExample>
{
    List<IExample> Examples { get; }

    IDataSetSpecification Specification { get; set; }

    IEnumerable<string> GetNonTargetAttributes();

    IEnumerable<string> GetPossibleAttributeValues(string attributeName);

    IDataSet EmptyDataSet();

    double CalculateGainFor(string parameterName);

    IDataSet MatchingDataSet(string attributeName, string attributeValue);

    double GetInformationFor();
}
