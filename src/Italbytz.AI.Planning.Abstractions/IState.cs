using System.Collections.Generic;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Planning;

public interface IState
{
    IList<ILiteral> Fluents { get; }

    IState Result(List<IActionSchema> actions);

    bool IsApplicable(IActionSchema action);
}
