using System.Collections.Generic;

namespace Italbytz.AI.Logic.Fol;

public interface IFolProof
{
    ISubstitution AnswerBindings { get; }

    IReadOnlyList<IFolProofStep> Steps { get; }
}

public interface IFolProofStep
{
    string Conclusion { get; }

    IReadOnlyList<string> Premises { get; }

    string InferenceRule { get; }
}

public interface IFolInferenceResult
{
    bool IsPossiblyFalse { get; }

    bool IsTrue { get; }

    bool IsUnknownDueToTimeout { get; }

    bool IsPartialResultDueToTimeout { get; }

    IReadOnlyList<IFolProof> Proofs { get; }
}