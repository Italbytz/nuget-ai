using System.Collections.Generic;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Planning;

public class HighLevelAction : ActionSchema
{
    public HighLevelAction(string name, List<ITerm>? variables, string precondition, string effect, List<List<IActionSchema>> refinements)
        : base(name, variables, precondition, effect)
    {
        Refinements = refinements;
    }

    public List<List<IActionSchema>> Refinements { get; }
}
