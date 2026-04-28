using Italbytz.AI.Agent;
using Italbytz.AI.Problem;

namespace Italbytz.AI.Search.Online;

public class OnlineDFSAgent<TPercept, TState, TAction> : SimpleAgent<TPercept, TAction>
    where TState : notnull
    where TAction : notnull
{
    private readonly IOnlineSearchProblem<TState, TAction> _problem;
    private readonly Func<TPercept?, TState> _perceptToState;
    private readonly Dictionary<TState, List<TAction>> _untried = new();
    private readonly Dictionary<TState, List<TState>> _unbacktracked = new();
    private readonly Dictionary<(TState, TAction), TState> _result = new();

    private TState? _previousState;
    private TAction? _previousAction;

    public OnlineDFSAgent(IOnlineSearchProblem<TState, TAction> problem, Func<TPercept?, TState> perceptToState)
    {
        _problem = problem;
        _perceptToState = perceptToState;
    }

    public override TAction? Act(TPercept? percept)
    {
        var state = _perceptToState(percept);

        if (_problem.GoalTest(state))
        {
            Alive = false;
            return default;
        }

        if (!_untried.ContainsKey(state))
        {
            _untried[state] = _problem.Actions(state).ToList();
        }

        if (_previousState is not null && _previousAction is not null)
        {
            var key = (_previousState, _previousAction);
            if (!_result.ContainsKey(key))
            {
                _result[key] = state;
                if (!_unbacktracked.ContainsKey(state))
                {
                    _unbacktracked[state] = new List<TState>();
                }

                if (!_unbacktracked[state].Contains(_previousState))
                {
                    _unbacktracked[state].Add(_previousState);
                }
            }
        }

        TAction? nextAction;
        if (_untried[state].Count > 0)
        {
            nextAction = _untried[state][0];
            _untried[state].RemoveAt(0);
        }
        else if (_unbacktracked.TryGetValue(state, out var backtrackStates) && backtrackStates.Count > 0)
        {
            var target = backtrackStates[^1];
            backtrackStates.RemoveAt(backtrackStates.Count - 1);
            nextAction = _result
                .Where(kvp => EqualityComparer<TState>.Default.Equals(kvp.Key.Item1, state)
                           && EqualityComparer<TState>.Default.Equals(kvp.Value, target))
                .Select(kvp => kvp.Key.Item2)
                .FirstOrDefault();
        }
        else
        {
            Alive = false;
            return default;
        }

        _previousState = state;
        _previousAction = nextAction;
        return nextAction;
    }
}