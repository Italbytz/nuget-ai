using System;

namespace Italbytz.AI.Planning.Fol;

public interface ILiteral : IEquatable<ILiteral>
{
    bool NegativeLiteral { get; }

    bool PositiveLiteral { get; }

    IAtomicSentence Atom { get; }

    ILiteral GetComplementaryLiteral();
}
