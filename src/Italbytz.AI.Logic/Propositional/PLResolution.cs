using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Logic.Propositional;

namespace Italbytz.AI.Logic.Propositional;

/// <summary>
/// PL-RESOLUTION (AIMA3e Fig. 7.12).
/// Proves α from KB by refutation: shows KB ∧ ¬α is unsatisfiable.
/// </summary>
public class PLResolution : IEntailmentChecker
{
    public bool IsEntailed(IPropKnowledgeBase kb, string query)
    {
        // KB ∧ ¬query in CNF
        var kbClauses = kb.GetClauses().ToList();
        var negQuery = new PropClause(new[] { new PropLiteral(query, false) });
        var clauses = new HashSet<string>(kbClauses.Concat(new[] { (IPropClause)negQuery })
            .Select(ClauseKey));

        while (true)
        {
            var clauseList = clauses.Select(ParseKey).ToList();
            var newClauses = new HashSet<string>();

            for (int i = 0; i < clauseList.Count; i++)
            {
                for (int j = i + 1; j < clauseList.Count; j++)
                {
                    var resolvents = Resolve(clauseList[i], clauseList[j]);
                    if (resolvents.Any(r => r.IsEmpty)) return true;
                    foreach (var r in resolvents)
                        if (!r.IsTautology)
                            newClauses.Add(ClauseKey(r));
                }
            }

            if (newClauses.IsSubsetOf(clauses)) return false;
            foreach (var c in newClauses) clauses.Add(c);
        }
    }

    private static IEnumerable<IPropClause> Resolve(IPropClause c1, IPropClause c2)
    {
        var results = new List<IPropClause>();
        foreach (var l1 in c1.Literals)
        {
            foreach (var l2 in c2.Literals)
            {
                if (l1.Symbol == l2.Symbol && l1.IsPositive != l2.IsPositive)
                {
                    var merged = c1.Literals.Where(l => !l.Equals(l1))
                        .Concat(c2.Literals.Where(l => !l.Equals(l2)))
                        .Distinct()
                        .ToList();
                    results.Add(new PropClause(merged));
                }
            }
        }
        return results;
    }

    // Serialization helpers for set de-duplication
    private static string ClauseKey(IPropClause c) =>
        string.Join(",", c.Literals
            .Select(l => (l.IsPositive ? "+" : "-") + l.Symbol)
            .OrderBy(x => x));

    private static IPropClause ParseKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return new PropClause(Enumerable.Empty<IPropLiteral>());
        return new PropClause(key.Split(',').Select(s =>
            (IPropLiteral)new PropLiteral(s[1..], s[0] == '+')));
    }
}
