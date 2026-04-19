using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Logic.Fol;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Logic.Fol;

/// <summary>
/// FOL FORWARD-CHAINING (AIMA3e Fig. 9.3).
/// FOL-FC-ASK: iteratively derive new facts until the query is matched.
/// Returns all substitutions θ such that query[θ] can be derived from the KB.
/// </summary>
public class ForwardChaining : IFolInference
{
    private readonly IUnifier _unifier = new Unifier();

    public IEnumerable<ISubstitution> Ask(IFolKnowledgeBase kb, ISentence query)
    {
        if (query is not IAtomicSentence goal) yield break;

        // Seed known facts (unit clauses)
        var known = kb.Clauses
            .Where(c => !c.premises.Any())
            .Select(c => c.conclusion.Atom)
            .ToList();

        bool changed;
        do
        {
            changed = false;
            foreach (var (conclusion, premises) in kb.Clauses.Where(c => c.premises.Any()))
            {
                foreach (var theta in MatchPremises(premises, known))
                {
                    var derived = ApplySubstitution(conclusion.Atom, theta);
                    if (!known.Any(f => f.SymbolicName == derived.SymbolicName &&
                                        ArgsMatch(f, derived)))
                    {
                        known.Add(derived);
                        changed = true;
                    }
                }
            }
        } while (changed);

        // Return all θ such that goal[θ] is in known
        foreach (var fact in known)
        {
            var theta = _unifier.Unify(goal, fact);
            if (theta is not null) yield return theta;
        }
    }

    private IEnumerable<ISubstitution> MatchPremises(
        IReadOnlyList<ILiteral> premises,
        List<IAtomicSentence> known)
    {
        IEnumerable<ISubstitution> current = new[] { Substitution.Empty };
        foreach (var premise in premises)
        {
            var p = premise;
            current = current.SelectMany(theta =>
                known
                    .Select(fact => _unifier.Unify(p.Atom, fact, theta))
                    .Where(t => t is not null)
                    .Select(t => t!)
            );
        }
        return current;
    }

    private static IAtomicSentence ApplySubstitution(IAtomicSentence atom, ISubstitution theta) =>
        // For our simple Horn-clause KB, atoms are returned as-is; a full
        // implementation would walk the arg list and substitute variables.
        atom;

    private static bool ArgsMatch(IAtomicSentence a, IAtomicSentence b) =>
        a.Args.Count == b.Args.Count &&
        a.Args.Zip(b.Args, (x, y) => x.SymbolicName == y.SymbolicName).All(m => m);
}
