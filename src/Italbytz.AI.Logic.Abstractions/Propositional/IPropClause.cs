using System.Collections.Generic;

namespace Italbytz.AI.Logic.Propositional;

/// <summary>A disjunction of literals in CNF form.</summary>
public interface IPropClause
{
    IReadOnlyList<IPropLiteral> Literals { get; }
    bool IsEmpty { get; }
    bool IsTautology { get; }
}
