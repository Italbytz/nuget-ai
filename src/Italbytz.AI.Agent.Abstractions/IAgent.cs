namespace Italbytz.AI.Agent;

/// <summary>
/// Agents interact with environments through percepts and actions.
/// </summary>
/// <typeparam name="TPercept">Type used to represent percepts.</typeparam>
/// <typeparam name="TAction">Type used to represent actions.</typeparam>
public interface IAgent<in TPercept, out TAction>
{
    bool Alive { get; }

    TAction? Act(TPercept? percept);
}
