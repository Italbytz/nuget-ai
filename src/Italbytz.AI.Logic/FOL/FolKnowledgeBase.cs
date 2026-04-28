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

    public void Tell(string clauseText, FolParser parser)
    {
        var sentence = parser.Parse(clauseText);
        var (conclusion, premises) = ToDefiniteClause(sentence);
        Tell(conclusion, premises);
    }

    public IEnumerable<ISubstitution> Ask(ISentence query) =>
        AskResult(query).Proofs.Select(proof => proof.AnswerBindings);

    public IFolInferenceResult AskResult(ISentence query) =>
        _defaultInference.AskResult(this, query);

    public IEnumerable<ISubstitution> Ask(string queryText, FolParser parser) =>
        Ask(parser.Parse(queryText));

    public IFolInferenceResult AskResult(string queryText, FolParser parser) =>
        AskResult(parser.Parse(queryText));

    private static (ILiteral conclusion, IReadOnlyList<ILiteral> premises) ToDefiniteClause(ISentence sentence)
    {
        sentence = StripSupportedQuantifiers(sentence);

        if (TryToLiteral(sentence, out var factLiteral))
        {
            return (factLiteral!, Array.Empty<ILiteral>());
        }

        if (sentence is IConnectedSentence { Connector: "=>" } implication
            && TryToLiteral(implication.Second, out var conclusion))
        {
            return (conclusion!, FlattenPremises(implication.First));
        }

        throw new ArgumentException("Only atomic facts and definite Horn clauses are supported.", nameof(sentence));
    }

    private static ISentence StripSupportedQuantifiers(ISentence sentence)
    {
        while (sentence is IQuantifiedSentence quantified)
        {
            if (!string.Equals(quantified.Quantifier, "FORALL", StringComparison.Ordinal))
            {
                throw new ArgumentException("Only universally quantified definite Horn clauses are supported.", nameof(sentence));
            }

            sentence = quantified.Sentence;
        }

        return sentence;
    }

    private static IReadOnlyList<ILiteral> FlattenPremises(ISentence sentence)
    {
        if (TryToLiteral(sentence, out var literal))
        {
            return new[] { literal! };
        }

        if (sentence is IConnectedSentence { Connector: "&" } conjunction)
        {
            return FlattenPremises(conjunction.First)
                .Concat(FlattenPremises(conjunction.Second))
                .ToList();
        }

        throw new ArgumentException("Only conjunctions of positive atomic premises are supported.", nameof(sentence));
    }

    private static bool TryToLiteral(ISentence sentence, out ILiteral? literal)
    {
        if (sentence is IAtomicSentence atomicSentence)
        {
            literal = new Literal(atomicSentence, false);
            return true;
        }

        if (sentence is ITermEquality equality)
        {
            literal = new Literal(new Predicate("=", new[] { equality.Left, equality.Right }), false);
            return true;
        }

        literal = null;
        return false;
    }
}
