namespace Italbytz.AI.Agent;

/// <summary>
/// Minimal base implementation for agents.
/// </summary>
/// <typeparam name="TPercept">Type used to represent percepts.</typeparam>
/// <typeparam name="TAction">Type used to represent actions.</typeparam>
public class SimpleAgent<TPercept, TAction> : IAgent<TPercept, TAction>
{
    public bool Alive { get; protected set; } = true;

    public virtual TAction? Act(TPercept? percept)
    {
        return default;
    }
}
