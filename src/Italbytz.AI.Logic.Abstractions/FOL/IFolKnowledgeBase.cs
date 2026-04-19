using System.Collections.Generic;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Logic.Fol;

/// <summary>A FOL knowledge base storing definite Horn clauses.</summary>
public interface IFolKnowledgeBase
{
    void Tell(ILiteral conclusion, IReadOnlyList<ILiteral> premises);
    IEnumerable<ISubstitution> Ask(ISentence query);
    IEnumerable<ILiteral> Conclusions { get; }
    IReadOnlyList<(ILiteral conclusion, IReadOnlyList<ILiteral> premises)> Clauses { get; }
}
