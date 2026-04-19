using System.Collections.Generic;

namespace Italbytz.AI.NLP;

/// <summary>A probabilistic context-free grammar (PCFG).</summary>
public interface IGrammar
{
    ILexicon Lexicon { get; }
    IReadOnlyList<IGrammarRule> Rules { get; }
    IReadOnlyList<string> NonTerminals { get; }
    int NonterminalIndex(string symbol);
}
