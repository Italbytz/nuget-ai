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
    private int _nextStandardizedClauseId;

    public IEnumerable<ISubstitution> Ask(IFolKnowledgeBase kb, ISentence query)
    {
        return AskResult(kb, query).Proofs.Select(proof => proof.AnswerBindings);
    }

    public IFolInferenceResult AskResult(IFolKnowledgeBase kb, ISentence query)
    {
        return query switch
        {
            IAtomicSentence goal => FolInferenceResult.FromProofs(BcOr(kb, goal, Substitution.Empty), goal, "BACKWARD_CHAINING"),
            ITermEquality equality => AskEquality(kb, equality),
            INotSentence notSentence => AskNegated(kb, notSentence),
            _ => FolInferenceResult.PossiblyFalse()
        };
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
        // Avoid recursive loops on transitivity-like equality rules in BC by using
        // a finite saturation pass over facts/rules for this specific predicate query.
        var derivedSubstitutions = new ForwardChaining().Ask(kb, equalityPredicate);
        return FolInferenceResult.FromProofs(
            derivedSubstitutions,
            equality,
            "BACKWARD_CHAINING_EQUALITY_FALLBACK",
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

    private IEnumerable<ISubstitution> BcOr(
        IFolKnowledgeBase kb,
        IAtomicSentence goal,
        ISubstitution theta)
    {
        var instantiatedGoal = SubstitutionApplier.Apply(goal, theta);
        foreach (var (conclusion, premises) in kb.Clauses)
        {
            var standardizedClause = StandardizeApart(conclusion.Atom, premises.Select(p => p.Atom));
            var unified = _unifier.Unify(standardizedClause.conclusion, instantiatedGoal, theta);
            if (unified is not null)
            {
                var goals = standardizedClause.premises.ToList();
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

        foreach (var thetaPrime in BcOr(kb, SubstitutionApplier.Apply(first, theta), theta))
            foreach (var thetaDoublePrime in BcAnd(kb, rest, thetaPrime))
                yield return thetaDoublePrime;
    }

    private (IAtomicSentence conclusion, IReadOnlyList<IAtomicSentence> premises) StandardizeApart(
        IAtomicSentence conclusion,
        IEnumerable<IAtomicSentence> premises)
    {
        var suffix = ++_nextStandardizedClauseId;
        var variableMap = new Dictionary<string, IVariable>();

        return (
            CloneAtom(conclusion, suffix, variableMap),
            premises.Select(premise => CloneAtom(premise, suffix, variableMap)).ToList());
    }

    private static IAtomicSentence CloneAtom(
        IAtomicSentence atom,
        int suffix,
        IDictionary<string, IVariable> variableMap)
    {
        return new Predicate(
            atom.SymbolicName,
            atom.Args.Select(arg => CloneTerm(arg, suffix, variableMap)).ToList());
    }

    private static ITerm CloneTerm(ITerm term, int suffix, IDictionary<string, IVariable> variableMap)
    {
        if (term is IVariable variable)
        {
            if (!variableMap.TryGetValue(variable.SymbolicName, out var renamed))
            {
                renamed = new Variable($"{variable.SymbolicName}_{suffix}");
                variableMap[variable.SymbolicName] = renamed;
            }

            return renamed;
        }

        if (term is IFunctionTerm function)
        {
            return new Function(
                function.SymbolicName,
                function.Args.Select(arg => CloneTerm(arg, suffix, variableMap)).ToList());
        }

        return term;
    }
}
