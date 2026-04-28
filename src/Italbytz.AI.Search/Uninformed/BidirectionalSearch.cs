using Italbytz.AI;
using Italbytz.AI.Abstractions;
using Italbytz.AI.Problem;
using Italbytz.AI.Search.Framework.QSearch;

namespace Italbytz.AI.Search.Uninformed;

public class BidirectionalSearch<TState, TAction> : ISearchForActions<TState, TAction>, ISearchForStates<TState, TAction>
    where TState : notnull
{
    private readonly TState _goalState;
    private readonly Func<TState, List<TAction>> _reverseActions;
    private readonly Func<TState, TAction, TState> _reverseResult;

    public BidirectionalSearch(
        TState goalState,
        Func<TState, List<TAction>> reverseActions,
        Func<TState, TAction, TState> reverseResult)
    {
        _goalState = goalState;
        _reverseActions = reverseActions;
        _reverseResult = reverseResult;
        Metrics = new Metrics();
        ClearMetrics();
    }

    public IMetrics Metrics { get; }

    public IEnumerable<TAction>? FindActions(IProblem<TState, TAction> problem)
    {
        var path = FindStatePath(problem);
        if (path is null) return null;

        var actions = new List<TAction>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            var state = path[i];
            var next = path[i + 1];
            var action = problem.Actions(state).FirstOrDefault(a => EqualityComparer<TState>.Default.Equals(problem.Result(state, a), next));
            if (action is null) return null;
            actions.Add(action);
        }

        Metrics.Set(QueueSearch<TState, TAction>.MetricPathCost, actions.Count);
        return actions;
    }

    public TState? FindState(IProblem<TState, TAction> problem)
    {
        var path = FindStatePath(problem);
        return path is null ? default : path[^1];
    }

    private List<TState>? FindStatePath(IProblem<TState, TAction> problem)
    {
        ClearMetrics();

        var startQueue = new Queue<TState>();
        var goalQueue = new Queue<TState>();
        var startParents = new Dictionary<TState, TState?>();
        var goalParents = new Dictionary<TState, TState?>();

        startQueue.Enqueue(problem.InitialState);
        goalQueue.Enqueue(_goalState);
        startParents[problem.InitialState] = default;
        goalParents[_goalState] = default;
        UpdateQueueMetrics(startQueue.Count + goalQueue.Count);

        while (startQueue.Count > 0 && goalQueue.Count > 0)
        {
            var meeting = ExpandFrontier(startQueue, startParents, goalParents, problem.Actions, problem.Result);
            if (meeting is not null)
            {
                return ReconstructPath(meeting, startParents, goalParents);
            }

            meeting = ExpandFrontier(goalQueue, goalParents, startParents, _reverseActions, _reverseResult);
            if (meeting is not null)
            {
                return ReconstructPath(meeting, startParents, goalParents);
            }
        }

        return null;
    }

    private TState? ExpandFrontier(
        Queue<TState> frontier,
        Dictionary<TState, TState?> thisParents,
        Dictionary<TState, TState?> otherParents,
        Func<TState, List<TAction>> actions,
        Func<TState, TAction, TState> result)
    {
        if (frontier.Count == 0) return default;

        var state = frontier.Dequeue();
        Metrics.IncrementInt(QueueSearch<TState, TAction>.MetricNodesExpanded);
        UpdateQueueMetrics(frontier.Count);

        foreach (var action in actions(state))
        {
            var successor = result(state, action);
            if (thisParents.ContainsKey(successor))
            {
                continue;
            }

            thisParents[successor] = state;
            if (otherParents.ContainsKey(successor))
            {
                return successor;
            }

            frontier.Enqueue(successor);
        }

        UpdateQueueMetrics(frontier.Count);
        return default;
    }

    private List<TState> ReconstructPath(
        TState meeting,
        Dictionary<TState, TState?> startParents,
        Dictionary<TState, TState?> goalParents)
    {
        var startPath = new List<TState>();
        var current = meeting;
        while (true)
        {
            startPath.Add(current);
            var parent = startParents[current];
            if (parent is null) break;
            current = parent;
        }
        startPath.Reverse();

        var goalPath = new List<TState>();
        current = meeting;
        while (goalParents.TryGetValue(current, out var parent) && parent is not null)
        {
            current = parent;
            goalPath.Add(current);
        }

        startPath.AddRange(goalPath);
        return startPath;
    }

    private void ClearMetrics()
    {
        Metrics.Set(QueueSearch<TState, TAction>.MetricNodesExpanded, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricQueueSize, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricMaxQueueSize, 0);
        Metrics.Set(QueueSearch<TState, TAction>.MetricPathCost, 0);
    }

    private void UpdateQueueMetrics(int queueSize)
    {
        Metrics.Set(QueueSearch<TState, TAction>.MetricQueueSize, queueSize);
        if (queueSize > Metrics.GetInt(QueueSearch<TState, TAction>.MetricMaxQueueSize))
        {
            Metrics.Set(QueueSearch<TState, TAction>.MetricMaxQueueSize, queueSize);
        }
    }
}