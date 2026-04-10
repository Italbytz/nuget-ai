using System.Collections.Generic;
using Italbytz.AI.Agent;
using Italbytz.AI.Problem;

namespace Italbytz.AI.Search.Agent;

/// <summary>
/// Agent that computes a plan with a search algorithm up front and then executes the resulting action sequence.
/// </summary>
public class SearchAgent<TPercept, TState, TAction> : SimpleAgent<TPercept, TAction>
{
    private readonly Queue<TAction> _actionsQueue;

    public SearchAgent(IProblem<TState, TAction> problem, ISearchForActions<TState, TAction> search)
    {
        var actions = search.FindActions(problem) ?? [];
        Actions = [.. actions];
        _actionsQueue = new Queue<TAction>(Actions);
        SearchMetrics = search.Metrics;
    }

    public List<TAction> Actions { get; }

    public bool Done => _actionsQueue.Count == 0;

    public Italbytz.AI.Abstractions.IMetrics SearchMetrics { get; }

    public override TAction? Act(TPercept? percept)
    {
        return _actionsQueue.Count > 0 ? _actionsQueue.Dequeue() : default;
    }
}
