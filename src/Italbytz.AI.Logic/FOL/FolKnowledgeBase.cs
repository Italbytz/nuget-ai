using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Logic.Fol;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Logic.Fol;

/// <summary>
/// A FOL knowledge base storing definite Horn clauses (head ← body).
/// Supports FOL forward and backward chaining.
/// </summary>
public class FolKnowledgeBase : IFolKnowledgeBase
{
    private readonly List<(ILiteral conclusion, IReadOnlyList<ILiteral> premises)> _clauses = new();
    private readonly IFolInference _defaultInference;

    public FolKnowledgeBase(IFolInference? inference = null)
    {
        _defaultInference = inference ?? new BackwardChaining();
    }

    public IReadOnlyList<(ILiteral conclusion, IReadOnlyList<ILiteral> premises)> Clauses => _clauses;

    public IEnumerable<ILiteral> Conclusions => _clauses.Select(c => c.conclusion);

    public void Tell(ILiteral conclusion, IReadOnlyList<ILiteral> premises) =>
        _clauses.Add((conclusion, premises));

    public IEnumerable<ISubstitution> Ask(ISentence query) =>
        _defaultInference.Ask(this, query);
}
