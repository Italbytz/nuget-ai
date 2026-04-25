using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Italbytz.EA.Individuals;
using Italbytz.EA.Searchspace;

namespace Italbytz.AI.ML.LogicGp;

/// <summary>
/// Postprocessing utility that extracts pairwise feature interactions from
/// the final GP population produced by a trained <see cref="LogicGpTrainer{TOutput}"/>.
///
/// Mirrors the GPASInteractions / setInteractionR postprocessor from the
/// original R/Java FREAK implementation (Nunkesser et al. 2007): counts
/// co-occurrences of feature index pairs within the same monomial across all
/// individuals in the final population, then filters by a minimum count and a
/// minimum ratio relative to each feature's individual occurrence count.
/// </summary>
public static class LogicGpInteractionAnalyzer
{
    /// <summary>
    /// Extracts pairwise feature interactions from a trained trainer's final
    /// population.
    /// </summary>
    /// <param name="finalPopulation">
    ///   The <c>FinalPopulation</c> of a trained <see cref="LogicGpTrainer{TOutput}"/>.
    /// </param>
    /// <param name="featureNames">
    ///   Optional array mapping feature index → display name.  When
    ///   <c>null</c>, names default to <c>"F0"</c>, <c>"F1"</c>, etc.
    /// </param>
    /// <param name="minOccurrences">
    ///   Minimum co-occurrence count to retain an interaction edge.
    ///   Mirrors the <c>occurences</c> parameter of <c>GPASInteractions</c>.
    /// </param>
    /// <param name="minRatio">
    ///   Minimum ratio of the pair count to the individual feature count
    ///   (<c>pairCount / min(countA, countB)</c>).
    ///   Mirrors the <c>ratio</c> parameter of <c>GPASInteractions</c>.
    /// </param>
    /// <param name="outDot">
    ///   Optional file path for a GraphViz DOT output file.
    /// </param>
    /// <param name="outCsv">
    ///   Optional file path for a CSV output file with columns
    ///   <c>feature_a,feature_b,count,ratio_a,ratio_b</c>.
    /// </param>
    /// <returns>
    ///   An <see cref="InteractionResult"/> with the filtered edges and raw counts.
    /// </returns>
    public static InteractionResult ExtractInteractions(
        global::Italbytz.AI.Evolutionary.Individuals.IIndividualList finalPopulation,
        string[]? featureNames = null,
        int minOccurrences = 10,
        double minRatio = 0.1,
        string? outDot = null,
        string? outCsv = null)
    {
        var featureCounts = new Dictionary<int, int>();
        var pairCounts = new Dictionary<(int, int), int>();

        foreach (var individual in finalPopulation)
        {
            if (individual.Genotype is not WeightedPolynomialGenotype<
                    SetLiteral<int>, int> genotype)
                continue;

            foreach (var monomial in genotype.Polynomial.Monomials)
            {
                var indices = monomial.Literals
                    .Select(l => l.Feature)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                foreach (var fi in indices)
                {
                    featureCounts.TryGetValue(fi, out var c);
                    featureCounts[fi] = c + 1;
                }

                for (var a = 0; a < indices.Count; a++)
                for (var b = a + 1; b < indices.Count; b++)
                {
                    var key = (indices[a], indices[b]);
                    pairCounts.TryGetValue(key, out var pc);
                    pairCounts[key] = pc + 1;
                }
            }
        }

        // --- filter --------------------------------------------------------
        var edges = new List<InteractionEdge>();
        foreach (var ((fi, fj), count) in pairCounts)
        {
            if (count < minOccurrences)
                continue;

            var countA = featureCounts.GetValueOrDefault(fi, 0);
            var countB = featureCounts.GetValueOrDefault(fj, 0);
            var ratioA = countA > 0 ? (double)count / countA : 0.0;
            var ratioB = countB > 0 ? (double)count / countB : 0.0;

            if (Math.Max(ratioA, ratioB) < minRatio)
                continue;

            edges.Add(new InteractionEdge(
                FeatureName(fi, featureNames),
                FeatureName(fj, featureNames),
                count,
                ratioA,
                ratioB));
        }

        // --- named maps for return value -----------------------------------
        var namedFeatureCounts = featureCounts.ToDictionary(
            kv => FeatureName(kv.Key, featureNames),
            kv => kv.Value);

        var namedPairCounts = pairCounts.ToDictionary(
            kv => (FeatureName(kv.Key.Item1, featureNames),
                   FeatureName(kv.Key.Item2, featureNames)),
            kv => kv.Value);

        var result = new InteractionResult(edges, namedFeatureCounts, namedPairCounts);

        // --- optional output -----------------------------------------------
        if (outDot is not null)
            WriteDot(result, outDot);
        if (outCsv is not null)
            WriteCsv(result, outCsv);

        return result;
    }

    // -----------------------------------------------------------------------
    // Output helpers
    // -----------------------------------------------------------------------

    private static void WriteDot(InteractionResult result, string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var sb = new StringBuilder();
        sb.AppendLine("graph interactions {");
        sb.AppendLine("  node [shape=ellipse];");

        var nodeNames = result.Edges
            .SelectMany(e => new[] { e.FeatureA, e.FeatureB })
            .Distinct()
            .OrderBy(n => n);

        foreach (var name in nodeNames)
        {
            var cnt = result.FeatureCounts.GetValueOrDefault(name, 0);
            var safeId = DotId(name);
            sb.AppendLine($"  {safeId} [label=\"{EscapeDotLabel(name)}\\n({cnt})\"];");
        }

        foreach (var edge in result.Edges)
        {
            var a = DotId(edge.FeatureA);
            var b = DotId(edge.FeatureB);
            sb.AppendLine($"  {a} -- {b} [label={edge.Count}];");
        }

        sb.AppendLine("}");
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }

    private static void WriteCsv(InteractionResult result, string path)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var lines = new List<string>
            { "feature_a,feature_b,count,ratio_a,ratio_b" };
        lines.AddRange(result.Edges.Select(e =>
            $"{e.FeatureA},{e.FeatureB},{e.Count}," +
            $"{e.RatioA:G6},{e.RatioB:G6}"));
        File.WriteAllLines(path, lines, Encoding.UTF8);
    }

    private static string FeatureName(int idx, string[]? featureNames)
        => featureNames is not null && idx < featureNames.Length
            ? featureNames[idx]
            : $"F{idx}";

    private static string DotId(string name)
        => $"\"{name.Replace("\"", "\\\"")}\"";

    private static string EscapeDotLabel(string name)
        => name.Replace("\"", "\\\"");
}

// ---------------------------------------------------------------------------
// Result types
// ---------------------------------------------------------------------------

/// <summary>A single filtered interaction edge.</summary>
/// <param name="FeatureA">Name of the first feature.</param>
/// <param name="FeatureB">Name of the second feature.</param>
/// <param name="Count">Co-occurrence count.</param>
/// <param name="RatioA">Ratio relative to FeatureA's individual count.</param>
/// <param name="RatioB">Ratio relative to FeatureB's individual count.</param>
public sealed record InteractionEdge(
    string FeatureA,
    string FeatureB,
    int Count,
    double RatioA,
    double RatioB);

/// <summary>Output from <see cref="LogicGpInteractionAnalyzer.ExtractInteractions"/>.</summary>
/// <param name="Edges">Filtered interaction edges.</param>
/// <param name="FeatureCounts">Individual feature occurrence counts.</param>
/// <param name="PairCounts">All pair co-occurrence counts (pre-filter).</param>
public sealed record InteractionResult(
    IReadOnlyList<InteractionEdge> Edges,
    IReadOnlyDictionary<string, int> FeatureCounts,
    IReadOnlyDictionary<(string, string), int> PairCounts);
