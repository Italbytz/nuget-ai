using Italbytz.AI.Agent;
using Italbytz.AI.Problem;

namespace Italbytz.AI.Search.Online;

public class LRTAStarAgent<TPercept, TState, TAction> : SimpleAgent<TPercept, TAction>
    where TState : notnull
    where TAction : notnull
{
    private readonly IOnlineSearchProblem<TState, TAction> _problem;
    private readonly Func<TPercept?, TState> _perceptToState;
    private readonly Func<TState, double> _heuristic;
    private readonly Dictionary<TState, double> _h = new();
    private readonly Dictionary<(TState, TAction), TState> _result = new();

    private TState? _previousState;
    private TAction? _previousAction;

    public LRTAStarAgent(
        IOnlineSearchProblem<TState, TAction> problem,
        Func<TPercept?, TState> perceptToState,
        Func<TState, double> heuristic)
    {
        _problem = problem;
        _perceptToState = perceptToState;
        _heuristic = heuristic;
    }

    public override TAction? Act(TPercept? percept)
    {
        var state = _perceptToState(percept);
        if (_problem.GoalTest(state))
        {
            Alive = false;
            return default;
        }

        if (!_h.ContainsKey(state))
        {
            _h[state] = _heuristic(state);
        }

        if (_previousState is not null && _previousAction is not null)
        {
            _result[(_previousState, _previousAction)] = state;
            _h[_previousState] = _problem.Actions(_previousState)
                .Select(action => LrtaCost(_previousState, action))
                .Min();
        }

        var actions = _problem.Actions(state);
        if (actions.Count == 0)
        {
            Alive = false;
            return default;
        }

        var nextAction = actions
            .OrderBy(action => LrtaCost(state, action))
            .First();

        _previousState = state;
        _previousAction = nextAction;
        return nextAction;
    }

    private double LrtaCost(TState state, TAction action)
    {
        if (_result.TryGetValue((state, action), out var successor))
        {
            if (!_h.ContainsKey(successor))
            {
                _h[successor] = _heuristic(successor);
            }

            return _problem.StepCosts(state, action, successor) + _h[successor];
        }

        return _heuristic(state);
    }
}