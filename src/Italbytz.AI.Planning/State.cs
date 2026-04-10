using System.Collections.Generic;
using System.Linq;
using Italbytz.AI.Planning.Fol;

namespace Italbytz.AI.Planning;

public class State : IState
{
    public State(IList<ILiteral> fluents)
    {
        Fluents = fluents.OrderBy(fluent => fluent.ToString()).ToList();
    }

    public State(string fluents) : this(Utils.Parse(fluents))
    {
    }

    public IList<ILiteral> Fluents { get; }

    public IState Result(List<IActionSchema> actions)
    {
        return actions.Aggregate((IState)this, (current, action) => ((State)current).Apply(action));
    }

    public bool IsApplicable(IActionSchema action)
    {
        return action.Precondition.All(literal => literal.PositiveLiteral
            ? Fluents.Contains(literal)
            : !Fluents.Contains(literal.GetComplementaryLiteral()));
    }

    private State Apply(IActionSchema action)
    {
        if (!IsApplicable(action))
        {
            return this;
        }

        var result = new List<ILiteral>(Fluents);
        foreach (var effect in action.Effect)
        {
            if (effect.PositiveLiteral)
            {
                if (!result.Contains(effect))
                {
                    result.Add(effect);
                }
            }
            else
            {
                result.Remove(effect.GetComplementaryLiteral());
            }
        }

        return new State(result);
    }
}
