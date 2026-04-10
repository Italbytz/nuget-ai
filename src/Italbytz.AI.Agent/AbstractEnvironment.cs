using System.Diagnostics;

namespace Italbytz.AI.Agent;

/// <summary>
/// Base class for discrete environments that manage a single active agent.
/// </summary>
/// <typeparam name="TPercept">Type used to represent percepts.</typeparam>
/// <typeparam name="TAction">Type used to represent actions.</typeparam>
public abstract class AbstractEnvironment<TPercept, TAction> : IEnvironment<TPercept, TAction>
{
    public IAgent<TPercept, TAction>? Agent { get; set; }

    public void Step()
    {
        Debug.Assert(Agent is not null, nameof(Agent) + " != null");
        if (Agent is null || !Agent.Alive)
        {
            return;
        }

        var percept = GetPerceptSeenBy(Agent);
        var action = Agent.Act(percept);
        if (action is not null)
        {
            Execute(Agent, action);
        }
    }

    protected abstract void Execute(IAgent<TPercept, TAction> agent, TAction action);

    protected abstract TPercept? GetPerceptSeenBy(IAgent<TPercept, TAction> agent);
}
