namespace Italbytz.AI.NLP;

/// <summary>
/// A probabilistic grammar rule: Left → Right1 [Right2].
/// Unary rules (Right2 = null) are lexical rules: NT → word.
/// Binary rules: NT → NT NT.
/// </summary>
public interface IGrammarRule
{
    string Left { get; }
    string Right1 { get; }
    string? Right2 { get; }
    double LogProbability { get; }
}
