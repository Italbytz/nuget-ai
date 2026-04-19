using System;
using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.NLP;

namespace Italbytz.AI.NLP;

/// <summary>
/// HITS (Hyperlink-Induced Topic Search) link-analysis algorithm (AIMA3e p. 872).
/// Alternates between authority and hub score updates until convergence.
/// Scores are normalised after each round.
/// </summary>
public class HITSRanker : IPageRanker
{
    private readonly double _convergenceThreshold;

    public HITSRanker(double convergenceThreshold = 1e-6)
    {
        _convergenceThreshold = convergenceThreshold;
    }

    public void Rank(IReadOnlyList<IPage> pages, int maxIterations = 100)
    {
        foreach (var p in pages) { p.HubScore = 1.0; p.AuthorityScore = 1.0; }

        var urlMap = pages.ToDictionary(p => p.Url);

        for (int iter = 0; iter < maxIterations; iter++)
        {
            // Update authority scores: a(p) = Σ_{q: q→p} h(q)
            double maxDelta = 0;
            var newAuthority = new Dictionary<string, double>();
            foreach (var p in pages)
            {
                double sum = pages
                    .Where(q => q.OutLinks.Contains(p.Url))
                    .Sum(q => q.HubScore);
                newAuthority[p.Url] = sum;
            }

            // Update hub scores: h(p) = Σ_{q: p→q} a(q)
            var newHub = new Dictionary<string, double>();
            foreach (var p in pages)
            {
                double sum = p.OutLinks
                    .Where(url => urlMap.ContainsKey(url))
                    .Sum(url => newAuthority[url]);
                newHub[p.Url] = sum;
            }

            // Normalise
            double normA = Math.Sqrt(newAuthority.Values.Sum(v => v * v));
            double normH = Math.Sqrt(newHub.Values.Sum(v => v * v));
            if (normA == 0) normA = 1;
            if (normH == 0) normH = 1;

            foreach (var p in pages)
            {
                double newA = newAuthority[p.Url] / normA;
                double newH = newHub[p.Url] / normH;
                maxDelta = Math.Max(maxDelta,
                    Math.Abs(newA - p.AuthorityScore) + Math.Abs(newH - p.HubScore));
                p.AuthorityScore = newA;
                p.HubScore = newH;
            }

            if (maxDelta < _convergenceThreshold) break;
        }
    }
}
