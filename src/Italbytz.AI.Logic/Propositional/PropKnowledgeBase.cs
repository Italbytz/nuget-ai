using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Logic.Propositional;

namespace Italbytz.AI.Logic.Propositional;

/// <summary>A propositional knowledge base in CNF clause form.</summary>
public class PropKnowledgeBase : IPropKnowledgeBase
{
    private readonly List<IPropClause> _clauses = new();

    public IEnumerable<string> Symbols =>
        _clauses.SelectMany(c => c.Literals.Select(l => l.Symbol)).Distinct();

    public void Tell(IPropSentence sentence)
    {
        var newClauses = CNFConverter.Convert(sentence);
        _clauses.AddRange(newClauses);
    }

    public IReadOnlyList<IPropClause> GetClauses() => _clauses;
}
