using System.Collections.Generic;

namespace Italbytz.AI.Logic.Propositional;

/// <summary>Finds a satisfying assignment for a CNF clause set, or null if unsatisfiable.</summary>
public interface ISatSolver
{
    IDictionary<string, bool>? FindModel(IReadOnlyList<IPropClause> clauses);
}
