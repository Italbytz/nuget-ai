using System.Collections.Generic;

namespace Italbytz.AI.Logic.Propositional;

/// <summary>A propositional knowledge base in clause form.</summary>
public interface IPropKnowledgeBase
{
    void Tell(IPropSentence sentence);
    IReadOnlyList<IPropClause> GetClauses();
    IEnumerable<string> Symbols { get; }
}
