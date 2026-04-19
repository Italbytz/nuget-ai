using System.Collections.Generic;

namespace Italbytz.AI.NLP;

/// <summary>
/// CYK chart parser for a PCFG in Chomsky Normal Form.
/// Returns a 3-D table where table[ntIdx, start, end] is the log-probability
/// of the most probable parse of words[start..end] as non-terminal ntIdx.
/// Double.NegativeInfinity signals no parse.
/// </summary>
public interface ICFGParser
{
    double[,,] Parse(IReadOnlyList<string> words, IGrammar grammar);
}
