using System.Collections.Generic;

namespace Italbytz.AI.NLP;

/// <summary>HITS link-analysis page ranker.</summary>
public interface IPageRanker
{
    void Rank(IReadOnlyList<IPage> pages, int maxIterations = 100);
}
