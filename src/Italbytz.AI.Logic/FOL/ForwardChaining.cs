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
        return AskResult(kb, query).Proofs.Select(proof => proof.AnswerBindings);
    }

    public IFolInferenceResult AskResult(IFolKnowledgeBase kb, ISentence query)
    {
        return query switch
        {
            IAtomicSentence goal => FolInferenceResult.FromProofs(AskAtomic(kb, goal), goal, "FORWARD_CHAINING"),
            ITermEquality equality => AskEquality(kb, equality),
            INotSentence notSentence => AskNegated(kb, notSentence),
            _ => FolInferenceResult.PossiblyFalse()
        };
    }

    private IEnumerable<ISubstitution> AskAtomic(IFolKnowledgeBase kb, IAtomicSentence goal)
    {
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
                foreach (var theta in MatchPremises(premises, known).ToList())
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

    private IFolInferenceResult AskEquality(IFolKnowledgeBase kb, ITermEquality equality)
    {
        var directUnification = _unifier.Unify(equality.Left, equality.Right);
        if (directUnification is not null)
        {
            return FolInferenceResult.FromProofs(
                new[] { directUnification },
                equality,
                "TERM_UNIFICATION");
        }

        var equalityPredicate = new Predicate("=", new[] { equality.Left, equality.Right });
        var derivedSubstitutions = AskAtomic(kb, equalityPredicate);
        return FolInferenceResult.FromProofs(
            derivedSubstitutions,
            equality,
            "FORWARD_CHAINING_EQUALITY",
            new[] { equalityPredicate.ToString() });
    }

    private IFolInferenceResult AskNegated(IFolKnowledgeBase kb, INotSentence notSentence)
    {
        var inner = AskResult(kb, notSentence.Negated);
        if (inner.IsUnknownDueToTimeout)
        {
            return FolInferenceResult.UnknownDueToTimeout();
        }

        if (inner.IsPartialResultDueToTimeout)
        {
            return FolInferenceResult.Partial(new[] { Substitution.Empty });
        }

        return inner.IsTrue
            ? FolInferenceResult.PossiblyFalse()
            : FolInferenceResult.TrueWithoutBindings(
                notSentence,
                "NEGATION_AS_FAILURE",
                new[] { notSentence.Negated.ToString()! });
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
                {
                    var instantiatedPremise = SubstitutionApplier.Apply(p.Atom, theta);
                    return known
                        .Select(fact => _unifier.Unify(instantiatedPremise, fact, theta))
                        .Where(t => t is not null)
                        .Select(t => t!);
                }
            );
        }
        return current;
    }

    private static IAtomicSentence ApplySubstitution(IAtomicSentence atom, ISubstitution theta) =>
        SubstitutionApplier.Apply(atom, theta);

    private static bool ArgsMatch(IAtomicSentence a, IAtomicSentence b) =>
        a.Args.Count == b.Args.Count &&
        a.Args.Zip(b.Args, (x, y) => x.SymbolicName == y.SymbolicName).All(m => m);
}
