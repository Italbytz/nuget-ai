using Italbytz.AI.NLP;

namespace Italbytz.AI.Demos.Web.Demos;

internal sealed record CykCellEntry(
    string NonTerminal,
    double LogProbability);

internal sealed record CykChartStep(
    int Number,
    int Span,
    int Start,
    int End,
    string Label,
    string Summary,
    IReadOnlyList<CykCellEntry> Entries);

internal sealed record CykExampleDemo(
    string Key,
    string Name,
    string Summary,
    IReadOnlyList<string> Words,
    bool Parsable,
    double StartProbability,
    IReadOnlyList<CykChartStep> Steps);

internal static class CykParserDemoFactory
{
    public static IReadOnlyList<CykExampleDemo> BuildExamples()
    {
        return
        [
            BuildExample(
                "Canonical",
                "Canonical parse",
                "The standard transitive sentence from the unit test has a full parse under the toy CNF grammar.",
                ["the", "dog", "sees", "the", "cat"]),
            BuildExample(
                "Short",
                "Short parse",
                "A short sentence that still reaches the start symbol and highlights unary lexical cells clearly.",
                ["the", "cat", "sees"]),
            BuildExample(
                "Unparsable",
                "Unparsable sequence",
                "This sequence leaves the top chart cell empty, which makes parser failure visible in the last span.",
                ["dog", "sees", "dog", "sees"])
        ];
    }

    public static CykExampleDemo CreateRandomExample(Random random)
    {
        var examples = BuildExamples();
        return examples[random.Next(examples.Count)];
    }

    public static string FormatProbability(double logProbability)
    {
        if (double.IsNegativeInfinity(logProbability))
        {
            return "-inf";
        }

        return Math.Exp(logProbability).ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
    }

    private static CykExampleDemo BuildExample(
        string key,
        string name,
        string summary,
        IReadOnlyList<string> words)
    {
        var grammar = BuildToyGrammar();
        var parser = new CYKParser();
        var table = parser.Parse(words, grammar);
        var steps = BuildSteps(words, grammar, table);
        var startProbability = table[grammar.NonterminalIndex("S"), 0, words.Count - 1];

        return new CykExampleDemo(
            key,
            name,
            summary,
            words,
            !double.IsNegativeInfinity(startProbability),
            startProbability,
            steps);
    }

    private static IReadOnlyList<CykChartStep> BuildSteps(IReadOnlyList<string> words, IGrammar grammar, double[,,] table)
    {
        var steps = new List<CykChartStep>();
        var number = 1;

        for (var span = 1; span <= words.Count; span++)
        {
            for (var start = 0; start <= words.Count - span; start++)
            {
                var end = start + span - 1;
                var entries = grammar.NonTerminals
                    .Select(nonTerminal => new CykCellEntry(
                        nonTerminal,
                        table[grammar.NonterminalIndex(nonTerminal), start, end]))
                    .Where(entry => !double.IsNegativeInfinity(entry.LogProbability))
                    .OrderByDescending(entry => entry.LogProbability)
                    .ToArray();
                var label = string.Join(' ', words.Skip(start).Take(span));
                var summary = entries.Length == 0
                    ? $"Span {span} over '{label}' yields no non-terminal under the toy grammar."
                    : $"Span {span} over '{label}' derives {string.Join(", ", entries.Select(entry => entry.NonTerminal))}.";

                steps.Add(new CykChartStep(number++, span, start, end, label, summary, entries));
            }
        }

        return steps;
    }

    private static IGrammar BuildToyGrammar()
    {
        var nonTerminals = new[] { "S", "NP", "VP", "Det", "N", "V" };
        var rules = new List<IGrammarRule>
        {
            new GrammarRule("S", "NP", "VP", Math.Log(1.0)),
            new GrammarRule("NP", "Det", "N", Math.Log(0.6)),
            new GrammarRule("VP", "V", "NP", Math.Log(0.7)),
            new GrammarRule("VP", "V", null, Math.Log(0.3)),
            new GrammarRule("Det", "the", null, Math.Log(1.0)),
            new GrammarRule("N", "dog", null, Math.Log(0.5)),
            new GrammarRule("N", "cat", null, Math.Log(0.5)),
            new GrammarRule("V", "sees", null, Math.Log(1.0))
        };

        return new Grammar(new Lexicon(Array.Empty<ILexicalEntry>()), rules, nonTerminals);
    }
}