namespace Italbytz.AI.NLP;

/// <summary>A lexical entry: a word with its POS tag and log-probability.</summary>
public interface ILexicalEntry
{
    string Word { get; }
    string PartOfSpeech { get; }
    double LogProbability { get; }
}
