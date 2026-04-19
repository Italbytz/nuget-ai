using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.NLP;

namespace Italbytz.AI.NLP;

public class LexicalEntry : ILexicalEntry
{
    public string Word { get; }
    public string PartOfSpeech { get; }
    public double LogProbability { get; }

    public LexicalEntry(string word, string partOfSpeech, double logProbability = 0.0)
    {
        Word = word;
        PartOfSpeech = partOfSpeech;
        LogProbability = logProbability;
    }
}

public class Lexicon : ILexicon
{
    private readonly List<ILexicalEntry> _entries;

    public Lexicon(IEnumerable<ILexicalEntry> entries)
    {
        _entries = entries.ToList();
    }

    public IEnumerable<ILexicalEntry> GetEntries(string partOfSpeech) =>
        _entries.Where(e => e.PartOfSpeech == partOfSpeech);

    public IEnumerable<ILexicalEntry> GetEntries(string word, string partOfSpeech) =>
        _entries.Where(e => e.Word == word && e.PartOfSpeech == partOfSpeech);
}

public class GrammarRule : IGrammarRule
{
    public string Left { get; }
    public string Right1 { get; }
    public string? Right2 { get; }
    public double LogProbability { get; }

    public GrammarRule(string left, string right1, string? right2 = null, double logProbability = 0.0)
    {
        Left = left;
        Right1 = right1;
        Right2 = right2;
        LogProbability = logProbability;
    }
}

public class Grammar : IGrammar
{
    private readonly List<string> _nonTerminals;
    private readonly Dictionary<string, int> _ntIndex;

    public ILexicon Lexicon { get; }
    public IReadOnlyList<IGrammarRule> Rules { get; }
    public IReadOnlyList<string> NonTerminals => _nonTerminals;

    public Grammar(ILexicon lexicon, IReadOnlyList<IGrammarRule> rules, IReadOnlyList<string> nonTerminals)
    {
        Lexicon = lexicon;
        Rules = rules;
        _nonTerminals = nonTerminals.ToList();
        _ntIndex = _nonTerminals.Select((nt, i) => (nt, i))
            .ToDictionary(pair => pair.nt, pair => pair.i);
    }

    public int NonterminalIndex(string symbol) => _ntIndex[symbol];
}
