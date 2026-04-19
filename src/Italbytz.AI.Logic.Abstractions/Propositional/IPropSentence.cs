using System.Collections.Generic;

namespace Italbytz.AI.Logic.Propositional;

/// <summary>A propositional logic sentence node.</summary>
public interface IPropSentence
{
    bool IsTrue(IDictionary<string, bool> model);
    bool IsFalse(IDictionary<string, bool> model);
    IEnumerable<string> Symbols { get; }
}
