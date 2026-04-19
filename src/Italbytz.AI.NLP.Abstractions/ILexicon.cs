using System.Collections.Generic;

namespace Italbytz.AI.NLP;

/// <summary>A lexicon mapping words to their lexical entries grouped by POS.</summary>
public interface ILexicon
{
    IEnumerable<ILexicalEntry> GetEntries(string partOfSpeech);
    IEnumerable<ILexicalEntry> GetEntries(string word, string partOfSpeech);
}
