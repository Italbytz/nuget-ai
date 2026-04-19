using System.Collections.Generic;

namespace Italbytz.AI.NLP;

/// <summary>A web page node for link analysis.</summary>
public interface IPage
{
    string Url { get; }
    double HubScore { get; set; }
    double AuthorityScore { get; set; }
    IReadOnlyList<string> InLinks { get; }
    IReadOnlyList<string> OutLinks { get; }
}
