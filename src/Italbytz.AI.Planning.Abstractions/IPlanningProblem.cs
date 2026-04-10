using System.Collections.Generic;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Planning;

public interface IPlanningProblem
{
    IList<ILiteral> Goal { get; }

    IState InitialState { get; }

    IEnumerable<IActionSchema> GetPropositionalisedActions();
}
