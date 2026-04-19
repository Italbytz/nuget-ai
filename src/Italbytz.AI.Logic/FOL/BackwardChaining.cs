using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Logic.Fol;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Logic.Fol;

/// <summary>
/// FOL BACKWARD-CHAINING (AIMA3e Fig. 9.6).
/// BC-ASK returns all substitutions θ such that KB ⊢ query[θ].
/// Works with definite Horn clauses.
/// </summary>
public class BackwardChaining : IFolInference
{
    private readonly IUnifier _unifier = new Unifier();

    public IEnumerable<ISubstitution> Ask(IFolKnowledgeBase kb, ISentence query)
    {
        if (query is not IAtomicSentence goal) yield break;
        foreach (var theta in BcOr(kb, goal, Substitution.Empty))
            yield return theta;
    }

    private IEnumerable<ISubstitution> BcOr(
        IFolKnowledgeBase kb,
        IAtomicSentence goal,
        ISubstitution theta)
    {
        foreach (var (conclusion, premises) in kb.Clauses)
        {
            var unified = _unifier.Unify(conclusion.Atom, goal, theta);
            if (unified is not null)
            {
                var goals = premises.Select(p => p.Atom).Cast<IAtomicSentence>().ToList();
                foreach (var sub in BcAnd(kb, goals, unified))
                    yield return sub;
            }
        }
    }

    private IEnumerable<ISubstitution> BcAnd(
        IFolKnowledgeBase kb,
        List<IAtomicSentence> goals,
        ISubstitution theta)
    {
        if (!goals.Any())
        {
            yield return theta;
            yield break;
        }

        var first = goals[0];
        var rest = goals.Skip(1).ToList();

        foreach (var thetaPrime in BcOr(kb, first, theta))
            foreach (var thetaDoublePrime in BcAnd(kb, rest, thetaPrime))
                yield return thetaDoublePrime;
    }
}
