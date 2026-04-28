using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Logic.Fol;

public class FolProofStep : IFolProofStep
{
    public FolProofStep(string conclusion, IReadOnlyList<string> premises, string inferenceRule)
    {
        Conclusion = conclusion;
        Premises = premises;
        InferenceRule = inferenceRule;
    }

    public string Conclusion { get; }

    public IReadOnlyList<string> Premises { get; }

    public string InferenceRule { get; }
}

public class FolProof : IFolProof
{
    public FolProof(ISubstitution answerBindings, IReadOnlyList<IFolProofStep>? steps = null)
    {
        AnswerBindings = answerBindings;
        Steps = steps ?? Array.Empty<IFolProofStep>();
    }

    public ISubstitution AnswerBindings { get; }

    public IReadOnlyList<IFolProofStep> Steps { get; }
}

public class FolInferenceResult : IFolInferenceResult
{
    private FolInferenceResult(
        bool isPossiblyFalse,
        bool isTrue,
        bool isUnknownDueToTimeout,
        bool isPartialResultDueToTimeout,
        IReadOnlyList<IFolProof> proofs)
    {
        IsPossiblyFalse = isPossiblyFalse;
        IsTrue = isTrue;
        IsUnknownDueToTimeout = isUnknownDueToTimeout;
        IsPartialResultDueToTimeout = isPartialResultDueToTimeout;
        Proofs = proofs;
    }

    public bool IsPossiblyFalse { get; }

    public bool IsTrue { get; }

    public bool IsUnknownDueToTimeout { get; }

    public bool IsPartialResultDueToTimeout { get; }

    public IReadOnlyList<IFolProof> Proofs { get; }

    public static IFolInferenceResult FromProofs(IEnumerable<ISubstitution> substitutions)
    {
        var proofs = substitutions.Select(substitution => (IFolProof)new FolProof(substitution)).ToList();
        return proofs.Count == 0
            ? PossiblyFalse()
            : new FolInferenceResult(false, true, false, false, proofs);
    }

    public static IFolInferenceResult FromProofs(
        IEnumerable<ISubstitution> substitutions,
        ISentence query,
        string inferenceRule,
        IReadOnlyList<string>? premises = null)
    {
        var step = new FolProofStep(
            query.ToString()!,
            premises ?? Array.Empty<string>(),
            inferenceRule);
        var proofs = substitutions
            .Select(substitution => (IFolProof)new FolProof(substitution, new[] { step }))
            .ToList();
        return proofs.Count == 0
            ? PossiblyFalse()
            : new FolInferenceResult(false, true, false, false, proofs);
    }

    public static IFolInferenceResult TrueWithoutBindings()
    {
        return new FolInferenceResult(false, true, false, false, new[] { new FolProof(Substitution.Empty) });
    }

    public static IFolInferenceResult TrueWithoutBindings(
        ISentence query,
        string inferenceRule,
        IReadOnlyList<string>? premises = null)
    {
        var step = new FolProofStep(
            query.ToString()!,
            premises ?? Array.Empty<string>(),
            inferenceRule);
        return new FolInferenceResult(
            false,
            true,
            false,
            false,
            new[] { new FolProof(Substitution.Empty, new[] { step }) });
    }

    public static IFolInferenceResult PossiblyFalse()
    {
        return new FolInferenceResult(true, false, false, false, Array.Empty<IFolProof>());
    }

    public static IFolInferenceResult UnknownDueToTimeout()
    {
        return new FolInferenceResult(false, false, true, false, Array.Empty<IFolProof>());
    }

    public static IFolInferenceResult Partial(IEnumerable<ISubstitution> substitutions)
    {
        return new FolInferenceResult(false, true, false, true, substitutions.Select(substitution => (IFolProof)new FolProof(substitution)).ToList());
    }
}