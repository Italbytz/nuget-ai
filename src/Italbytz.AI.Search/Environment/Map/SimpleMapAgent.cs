using Italbytz.AI.Abstractions;
using Italbytz.AI.Agent;

namespace Italbytz.AI.Search.Environment.Map;

public class SimpleMapAgent : SimpleAgent<DynamicPercept, MoveToAction>
{
    private readonly ExtendableMap _map;
    private readonly ISearchForActions<string, MoveToAction> _search;
    private readonly List<string>? _goals;
    private readonly Queue<MoveToAction> _plannedActions = new();
    private readonly Func<DynamicPercept, string?> _perceptToState = MapFunctions.CreatePerceptToStateFunction();
    private Action<string>? _notifier;
    private int _nextGoalIndex;

    public SimpleMapAgent(ExtendableMap map, ISearchForActions<string, MoveToAction> search)
    {
        _map = map;
        _search = search;
    }

    public SimpleMapAgent(ExtendableMap map, ISearchForActions<string, MoveToAction> search, string goal)
        : this(map, search, new[] { goal })
    {
    }

    public SimpleMapAgent(ExtendableMap map, ISearchForActions<string, MoveToAction> search, IEnumerable<string> goals)
    {
        _map = map;
        _search = search;
        _goals = goals.ToList();
    }

    public string? CurrentLocation { get; private set; }

    public IMetrics SearchMetrics => _search.Metrics;

    public SimpleMapAgent SetNotifier(Action<string> notifier)
    {
        _notifier = notifier;
        return this;
    }

    public override MoveToAction? Act(DynamicPercept? percept)
    {
        if (percept is not null)
        {
            CurrentLocation = _perceptToState(percept);
        }

        if (_plannedActions.Count == 0)
        {
            var goal = FormulateGoal();
            if (goal is null || CurrentLocation is null || string.Equals(CurrentLocation, goal, StringComparison.Ordinal))
            {
                return default;
            }

            _notifier?.Invoke($"CurrentLocation=In({CurrentLocation}), Goal=In({goal})");

            var problem = MapFunctions.CreateProblem(_map, CurrentLocation, goal);
            var actions = _search.FindActions(problem);
            if (actions is null)
            {
                return default;
            }

            foreach (var action in actions)
            {
                _plannedActions.Enqueue(action);
            }

            _notifier?.Invoke($"Search{_search.Metrics}");
        }

        return _plannedActions.Count > 0 ? _plannedActions.Dequeue() : default;
    }

    private string? FormulateGoal()
    {
        if (_goals is null)
        {
            return _map.RandomlyGenerateDestination();
        }

        if (_nextGoalIndex >= _goals.Count)
        {
            return null;
        }

        return _goals[_nextGoalIndex++];
    }
}